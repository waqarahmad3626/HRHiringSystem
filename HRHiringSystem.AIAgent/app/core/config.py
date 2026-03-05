from pydantic_settings import BaseSettings
from functools import lru_cache


class Settings(BaseSettings):
    """Application settings loaded from environment variables"""
    
    # API Settings
    app_name: str = "HRHiring AI Agent"
    debug: bool = False
    
    # Gemini API Settings
    gemini_api_key: str = ""
    gemini_model: str = "gemma-3-27b-it"
    gemini_temperature: float = 0.1
    
    # .NET API Settings
    dotnet_api_url: str = "http://localhost:8080"
    
    # Azure Blob Storage
    azure_blob_connection_string: str = ""
    
    # Scoring thresholds
    score_accepted_threshold: int = 80
    score_hr_review_threshold: int = 65

    # RAG settings
    rag_enabled: bool = True
    rag_persist_directory: str = "./rag_store"
    rag_collection_prefix: str = "job_kb"
    rag_top_k: int = 4
    rag_max_context_chars: int = 2500
    gemini_embedding_model: str = "models/text-embedding-004"
    
    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
        case_sensitive = False


@lru_cache()
def get_settings() -> Settings:
    """Get cached settings instance"""
    return Settings()
