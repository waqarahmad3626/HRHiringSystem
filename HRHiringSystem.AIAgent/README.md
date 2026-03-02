# HR Hiring System - AI Agent

FastAPI-based AI Agent for CV parsing, candidate evaluation, and interview question generation.

## Features

- **CV Parsing**: Extract candidate information (name, email, phone) from PDF/DOCX files
- **AI Evaluation**: Score candidates against job requirements using LLM
- **Skill Matching**: Compare candidate skills with job requirements
- **Interview Questions**: Generate relevant interview questions based on job and candidate profile
- **Self-Reflection**: AI validates its own reasoning for accuracy

## Tech Stack

- Python 3.11+
- FastAPI
- Google Gemini 1.5 Pro
- LangChain for agent orchestration
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
```

4. Run the application:
```bash
uvicorn app.main:app --host 0.0.0.0 --port 8001 --reload
```

## API Endpoints

- `POST /api/cv/parse` - Parse CV and extract candidate info
- `POST /api/evaluate` - Full AI evaluation of candidate against job
- `GET /health` - Health check endpoint

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
│   └── dotnet_client.py # .NET API client
└── schemas/
    ├── cv_data.py      # Pydantic models
    └── evaluation.py   # Evaluation schemas
```
