import hashlib
import logging
import re
from dataclasses import dataclass
from typing import Any

import google.generativeai as genai

from app.core.config import get_settings

logger = logging.getLogger(__name__)


@dataclass
class KnowledgeDocument:
    doc_id: str
    text: str
    metadata: dict[str, Any]


class RagService:
    """RAG service using Gemini embeddings + ChromaDB vector store."""

    def __init__(self):
        self.settings = get_settings()
        self.enabled = self.settings.rag_enabled

        self.client = None
        if self.enabled:
            try:
                import chromadb

                self.client = chromadb.PersistentClient(path=self.settings.rag_persist_directory)
                logger.info(
                    "RAG enabled with ChromaDB at %s (top_k=%s)",
                    self.settings.rag_persist_directory,
                    self.settings.rag_top_k,
                )
            except Exception as exc:
                self.enabled = False
                logger.warning("RAG disabled because ChromaDB could not be initialized: %s", exc)

    def build_context(
        self,
        *,
        job_id: str,
        job_title: str,
        job_requirements: str,
        required_skills: list[str],
        cv_text: str,
    ) -> str:
        if not self.enabled or self.client is None:
            return ""

        try:
            collection = self._get_collection(job_id)
            self._upsert_bootstrap_documents(
                collection=collection,
                job_id=job_id,
                job_title=job_title,
                job_requirements=job_requirements,
                required_skills=required_skills,
            )

            query = (
                f"Role: {job_title}\n"
                f"Required Skills: {', '.join(required_skills[:20])}\n"
                f"CV Excerpt:\n{cv_text[:2200]}"
            )
            query_embedding = self._embed_text(query, task_type="retrieval_query")

            result = collection.query(
                query_embeddings=[query_embedding],
                n_results=self.settings.rag_top_k,
                include=["documents", "metadatas", "distances"],
            )

            documents = (result.get("documents") or [[]])[0]
            metadatas = (result.get("metadatas") or [[]])[0]
            distances = (result.get("distances") or [[]])[0]

            if not documents:
                return ""

            context_parts: list[str] = []
            total_chars = 0
            for index, doc in enumerate(documents):
                source = "knowledge"
                similarity = "n/a"

                if index < len(metadatas) and metadatas[index]:
                    source = str(metadatas[index].get("source_type", source))

                if index < len(distances) and distances[index] is not None:
                    similarity = f"distance={float(distances[index]):.3f}"

                entry = f"[{source} | {similarity}] {doc}".strip()
                if total_chars + len(entry) > self.settings.rag_max_context_chars:
                    break

                context_parts.append(entry)
                total_chars += len(entry)

            return "\n\n".join(context_parts)

        except Exception as exc:
            logger.warning("RAG context retrieval failed: %s", exc)
            return ""

    def index_evaluation_report(
        self,
        *,
        job_id: str,
        application_id: str,
        job_title: str,
        status: str,
        score: int,
        summary_reasoning: str,
        matched_skills: list[str],
        missing_skills: list[str],
    ) -> None:
        if not self.enabled or self.client is None:
            return

        try:
            collection = self._get_collection(job_id)
            report_text = (
                f"Job Title: {job_title}\n"
                f"Application ID: {application_id}\n"
                f"Score: {score}\n"
                f"Status: {status}\n"
                f"Matched Skills: {', '.join(matched_skills[:25])}\n"
                f"Missing Skills: {', '.join(missing_skills[:25])}\n"
                f"Reasoning: {summary_reasoning[:1800]}"
            )

            doc = KnowledgeDocument(
                doc_id=f"evaluation::{application_id}",
                text=report_text,
                metadata={
                    "source_type": "historical_evaluation",
                    "job_id": job_id,
                    "application_id": application_id,
                    "status": status,
                    "score": int(score),
                },
            )
            self._upsert_documents(collection, [doc])
        except Exception as exc:
            logger.warning("Failed to index evaluation report for RAG: %s", exc)

    def _upsert_bootstrap_documents(
        self,
        *,
        collection,
        job_id: str,
        job_title: str,
        job_requirements: str,
        required_skills: list[str],
    ) -> None:
        documents: list[KnowledgeDocument] = [
            KnowledgeDocument(
                doc_id=f"job::{job_id}::title",
                text=f"Job Title: {job_title}",
                metadata={"source_type": "role_metadata", "job_id": job_id},
            ),
            KnowledgeDocument(
                doc_id=f"job::{job_id}::requirements",
                text=job_requirements[:4000],
                metadata={"source_type": "role_requirements", "job_id": job_id},
            ),
            KnowledgeDocument(
                doc_id=f"job::{job_id}::skills",
                text="Required Skills: " + ", ".join(required_skills[:120]),
                metadata={"source_type": "required_skills", "job_id": job_id},
            ),
        ]

        documents.extend(self._skill_taxonomy_documents())
        self._upsert_documents(collection, documents)

    def _upsert_documents(self, collection, documents: list[KnowledgeDocument]) -> None:
        valid_documents = [doc for doc in documents if doc.text and doc.text.strip()]
        if not valid_documents:
            return

        ids = [doc.doc_id for doc in valid_documents]
        texts = [doc.text for doc in valid_documents]
        embeddings = [self._embed_text(text, task_type="retrieval_document") for text in texts]
        metadatas = [doc.metadata for doc in valid_documents]

        collection.upsert(ids=ids, documents=texts, embeddings=embeddings, metadatas=metadatas)

    def _embed_text(self, text: str, task_type: str) -> list[float]:
        response = genai.embed_content(
            model=self.settings.gemini_embedding_model,
            content=text,
            task_type=task_type,
        )
        embedding = response.get("embedding")
        if embedding is None:
            raise ValueError("Embedding API did not return an embedding vector")
        return embedding

    def _get_collection(self, job_id: str):
        safe_job_id = re.sub(r"[^a-zA-Z0-9_-]", "_", job_id)[:80]
        collection_name = f"{self.settings.rag_collection_prefix}_{safe_job_id}"
        return self.client.get_or_create_collection(name=collection_name)

    def _skill_taxonomy_documents(self) -> list[KnowledgeDocument]:
        taxonomy_buckets = {
            "taxonomy::languages": "Programming Languages: Python, C#, Java, JavaScript, TypeScript, Go, Rust, SQL.",
            "taxonomy::backend": "Backend Skills: REST APIs, microservices, event-driven architecture, async processing, caching, authentication, authorization.",
            "taxonomy::cloud": "Cloud and DevOps: Docker, Kubernetes, Azure, AWS, CI/CD, monitoring, observability, infrastructure as code.",
            "taxonomy::data": "Data Skills: relational modeling, NoSQL, query optimization, ETL, data validation, indexing, vector search.",
            "taxonomy::soft": "Professional Skills: collaboration, communication, ownership, problem-solving, mentoring, stakeholder management.",
        }

        docs: list[KnowledgeDocument] = []
        for doc_id, text in taxonomy_buckets.items():
            docs.append(
                KnowledgeDocument(
                    doc_id=doc_id,
                    text=text,
                    metadata={
                        "source_type": "skill_taxonomy",
                        "hash": hashlib.md5(text.encode("utf-8")).hexdigest(),
                    },
                )
            )
        return docs


rag_service = RagService()
