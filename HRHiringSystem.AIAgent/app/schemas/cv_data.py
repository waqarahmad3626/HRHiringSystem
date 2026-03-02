from pydantic import BaseModel, Field
from typing import Optional


class CvParseRequest(BaseModel):
    """Request model for CV parsing - file is sent as multipart form data"""
    pass  # File handled separately


class CvParseResponse(BaseModel):
    """Response model for CV parsing"""
    first_name: Optional[str] = Field(None, alias="firstName")
    last_name: Optional[str] = Field(None, alias="lastName")
    email: Optional[str] = None
    phone: Optional[str] = None
    
    class Config:
        populate_by_name = True


class CvData(BaseModel):
    """Internal model for parsed CV data"""
    raw_text: str
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    email: Optional[str] = None
    phone: Optional[str] = None
    skills: list[str] = Field(default_factory=list)
    education: list[str] = Field(default_factory=list)
    experience: list[str] = Field(default_factory=list)
    years_of_experience: int = 0
