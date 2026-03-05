# HR Hiring System - AI Agent

FastAPI-based AI Agent for CV parsing, candidate evaluation, and interview question generation.

## Features

- **CV Parsing**: Extract candidate information (name, email, phone) from PDF/DOCX files
- **AI Evaluation**: Score candidates against job requirements using LLM
- **Adaptive Orchestration**: Dynamically chooses lightweight or full extraction path based on CV/job fit
- **RAG Retrieval**: Retrieve semantically similar historical evaluations and role knowledge via embeddings + ChromaDB
- **Skill Matching**: Compare candidate skills with job requirements
- **Interview Questions**: Generate relevant interview questions based on job and candidate profile
- **Self-Reflection**: AI validates its own reasoning for accuracy

## Tech Stack

- Python 3.11+
- FastAPI
- Google Gemini 1.5 Pro
- Gemini `text-embedding-004` for semantic retrieval
- LangChain for agent orchestration
- ChromaDB vector store
- PyPDF2 / python-docx for CV parsing
- Azure Blob Storage SDK

## Setup

1. Create virtual environment:
```bash
python -m venv venv
source venv/bin/activate  # Linux/Mac
.\venv\Scripts\activate   # Windows
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

3. Set environment variables:
```bash
export GEMINI_API_KEY=your-api-key
export DOTNET_API_URL=http://localhost:8080
export AZURE_BLOB_CONNECTION_STRING=your-connection-string
export RAG_ENABLED=true
export RAG_PERSIST_DIRECTORY=./rag_store
export RAG_TOP_K=4
```

4. Run the application:
```bash
uvicorn app.main:app --host 0.0.0.0 --port 8001 --reload
```

## API Endpoints

- `POST /api/cv/parse` - Parse CV and extract candidate info
- `POST /api/evaluate` - Full AI evaluation of candidate against job
- `GET /health` - Health check endpoint

## Evaluation Flow

1. Parse CV text from blob storage
2. Build adaptive analysis plan (lightweight/full)
3. Retrieve semantic context from ChromaDB knowledge base
4. Score skills, education, and experience with retrieved context
5. Reflect and validate score/status
6. Generate interview questions
7. Index completed evaluation report back into vector store

## Docker

```bash
docker build -t hrhiring-ai-agent .
docker run -p 8001:8001 -e OPENAI_API_KEY=xxx hrhiring-ai-agent
```

## Architecture

```
app/
├── main.py              # FastAPI entry point
├── core/
│   ├── config.py       # Configuration settings
│   └── dependencies.py # Dependency injection
├── api/
│   └── routes/
│       ├── parsing.py  # CV parsing endpoints
│       └── evaluation.py # AI evaluation endpoints
├── agents/
│   ├── hiring_agent.py # Main AI orchestrator
│   └── reflection.py   # Self-reflection logic
├── tools/
│   ├── cv_parser.py    # CV text extraction
│   ├── skill_extractor.py # NLP skill extraction
│   ├── scoring_tool.py # Candidate scoring
│   └── question_generator.py # Interview questions
├── services/
│   ├── blob_service.py # Azure Blob download
│   ├── dotnet_client.py # .NET API client
│   └── rag_service.py # Embeddings + vector retrieval service
└── schemas/
    ├── cv_data.py      # Pydantic models
    └── evaluation.py   # Evaluation schemas
```
