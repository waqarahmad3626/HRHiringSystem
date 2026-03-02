from fastapi import APIRouter, UploadFile, File, HTTPException

from app.core.config import get_settings
from app.core.llm_client import llm_client
from app.schemas.cv_data import CvParseResponse
from app.tools.cv_parser import cv_parser

router = APIRouter(prefix="/api/cv", tags=["CV Parsing"])


@router.post("/parse", response_model=CvParseResponse)
async def parse_cv(file: UploadFile = File(...)):
    """
    Parse a CV file and extract candidate information.
    
    Supports: PDF, DOC, DOCX
    
    Returns:
    - firstName: Candidate's first name
    - lastName: Candidate's last name  
    - email: Email address
    - phone: Phone number
    """
    # Validate file type
    allowed_extensions = ['.pdf', '.doc', '.docx']
    filename = file.filename or "unknown.pdf"
    ext = '.' + filename.split('.')[-1].lower() if '.' in filename else ''
    
    if ext not in allowed_extensions:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid file type. Allowed: {', '.join(allowed_extensions)}"
        )
    
    # Read file content
    content = await file.read()
    
    if not content:
        raise HTTPException(status_code=400, detail="Empty file uploaded")
    
    # Parse CV
    try:
        cv_data = cv_parser.parse(content, filename)
        
        # If basic parsing didn't extract name well, use LLM
        if not cv_data.first_name or not cv_data.last_name:
            cv_data = await _enhance_with_llm(cv_data)
        
        return CvParseResponse(
            first_name=cv_data.first_name,
            last_name=cv_data.last_name,
            email=cv_data.email,
            phone=cv_data.phone
        )
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Failed to parse CV: {str(e)}"
        )


async def _enhance_with_llm(cv_data) -> object:
    """Use LLM to extract name if regex failed"""
    settings = get_settings()
    
    if not settings.gemini_api_key:
        return cv_data
    
    try:
        prompt = f"""Extract the person's name from this CV text. 
Return in format: FirstName|LastName
If you can't determine the name, return: Unknown|Unknown

CV Text (first 2000 chars):
{cv_data.raw_text[:2000]}

Name (FirstName|LastName):"""

        system = "Extract name from CV. Return only FirstName|LastName format."
        
        response = llm_client.generate(prompt, system, temperature=0.1, max_tokens=50)
        
        name_parts = response.strip().split('|')
        if len(name_parts) == 2:
            cv_data.first_name = name_parts[0].strip() if name_parts[0].strip() != "Unknown" else cv_data.first_name
            cv_data.last_name = name_parts[1].strip() if name_parts[1].strip() != "Unknown" else cv_data.last_name
    except Exception as e:
        print(f"LLM name extraction failed: {e}")
    
    return cv_data
