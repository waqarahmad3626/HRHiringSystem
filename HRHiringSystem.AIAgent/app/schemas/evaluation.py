from pydantic import BaseModel, Field
from typing import Optional


class EvaluationRequest(BaseModel):
    """Request model for AI evaluation"""
    candidate_id: str = Field(..., alias="candidateId")
    job_id: str = Field(..., alias="jobId")
    application_id: str = Field(..., alias="applicationId")
    cv_url: str = Field(..., alias="cvUrl")
    job_title: str = Field(..., alias="jobTitle")
    job_description: str = Field(..., alias="jobDescription")
    job_requirements: str = Field(..., alias="jobRequirements")
    job_required_skills: str = Field(..., alias="jobRequiredSkills")  # JSON array string
    job_experience_years: int = Field(..., alias="jobExperienceYears")
    job_education: Optional[str] = Field(None, alias="jobEducation")
    
    class Config:
        populate_by_name = True


class SkillsAnalysis(BaseModel):
    """Skills analysis result"""
    matched_skills: list[str] = Field(default_factory=list, alias="matchedSkills")
    missing_skills: list[str] = Field(default_factory=list, alias="missingSkills")
    match_percentage: int = Field(0, alias="matchPercentage")
    details: str = ""
    
    class Config:
        populate_by_name = True


class EducationAnalysis(BaseModel):
    """Education analysis result"""
    required: str = ""
    candidate: str = ""
    match: bool = False
    score: int = 0
    
    class Config:
        populate_by_name = True


class ExperienceAnalysis(BaseModel):
    """Experience analysis result"""
    required_years: int = Field(0, alias="requiredYears")
    candidate_years: int = Field(0, alias="candidateYears")
    relevant_experience: list[str] = Field(default_factory=list, alias="relevantExperience")
    score: int = 0
    
    class Config:
        populate_by_name = True


class EvaluationSummary(BaseModel):
    """Evaluation summary"""
    decision: str = ""
    reasoning: str = ""
    strengths: list[str] = Field(default_factory=list)
    concerns: list[str] = Field(default_factory=list)
    
    class Config:
        populate_by_name = True


class InterviewQuestion(BaseModel):
    """Interview question with expected answer"""
    category: str = ""
    question: str = ""
    difficulty: str = ""
    expected_answer: str = Field("", alias="expectedAnswer")
    
    class Config:
        populate_by_name = True


class AIMetadata(BaseModel):
    """AI processing metadata"""
    model: str = ""
    processing_time_ms: int = Field(0, alias="processingTimeMs")
    reflection_iterations: int = Field(0, alias="reflectionIterations")
    
    class Config:
        populate_by_name = True


class EvaluationResponse(BaseModel):
    """Full evaluation response"""
    score: int = 0
    status: str = ""  # Accepted, HRReview, Rejected
    skills_analysis: SkillsAnalysis = Field(default_factory=SkillsAnalysis, alias="skillsAnalysis")
    education_analysis: EducationAnalysis = Field(default_factory=EducationAnalysis, alias="educationAnalysis")
    experience_analysis: ExperienceAnalysis = Field(default_factory=ExperienceAnalysis, alias="experienceAnalysis")
    summary: EvaluationSummary = Field(default_factory=EvaluationSummary)
    interview_questions: list[InterviewQuestion] = Field(default_factory=list, alias="interviewQuestions")
    ai_metadata: AIMetadata = Field(default_factory=AIMetadata, alias="aiMetadata")
    
    class Config:
        populate_by_name = True
