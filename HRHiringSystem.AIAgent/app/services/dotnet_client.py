import httpx
import logging
from typing import Any

from app.core.config import get_settings
from app.schemas.evaluation import EvaluationResponse, EvaluationRequest

logger = logging.getLogger(__name__)


class DotNetClient:
    """HTTP client for communicating with the .NET API"""
    
    def __init__(self):
        self.settings = get_settings()
        self.base_url = self.settings.dotnet_api_url
    
    async def save_evaluation_report(self, request: EvaluationRequest, 
                                     evaluation: EvaluationResponse) -> dict:
        """
        Save evaluation report to .NET API (which stores in MongoDB)
        and updates the job application status in SQL Server.
        """
        # Build the request body matching SaveEvaluationReportRequest in .NET
        report_data = {
            "candidateId": request.candidate_id,
            "jobId": request.job_id,
            "applicationId": request.application_id,
            "score": evaluation.score,
            "status": evaluation.status,
            "skillsAnalysis": {
                "matchedSkills": evaluation.skills_analysis.matched_skills,
                "missingSkills": evaluation.skills_analysis.missing_skills,
                "matchPercentage": evaluation.skills_analysis.match_percentage,
                "details": evaluation.skills_analysis.details
            },
            "educationAnalysis": {
                "required": evaluation.education_analysis.required,
                "candidate": evaluation.education_analysis.candidate,
                "match": evaluation.education_analysis.match,
                "score": evaluation.education_analysis.score
            },
            "experienceAnalysis": {
                "requiredYears": evaluation.experience_analysis.required_years,
                "candidateYears": evaluation.experience_analysis.candidate_years,
                "relevantExperience": evaluation.experience_analysis.relevant_experience,
                "score": evaluation.experience_analysis.score
            },
            "summary": {
                "decision": evaluation.summary.decision,
                "reasoning": evaluation.summary.reasoning,
                "strengths": evaluation.summary.strengths,
                "concerns": evaluation.summary.concerns
            },
            "interviewQuestions": [
                {
                    "category": q.category,
                    "question": q.question,
                    "difficulty": q.difficulty,
                    "expectedAnswer": q.expected_answer
                }
                for q in evaluation.interview_questions
            ],
            "aiMetadata": {
                "model": evaluation.ai_metadata.model,
                "processingTimeMs": evaluation.ai_metadata.processing_time_ms,
                "reflectionIterations": evaluation.ai_metadata.reflection_iterations
            }
        }
        
        # Debug: Log what we're sending
        if evaluation.interview_questions:
            first_q = evaluation.interview_questions[0]
            logger.info(f"Sending {len(evaluation.interview_questions)} questions. First question expected_answer: '{first_q.expected_answer[:100] if first_q.expected_answer else 'EMPTY'}...'")
        
        async with httpx.AsyncClient() as client:
            try:
                response = await client.post(
                    f"{self.base_url}/api/reports",
                    json=report_data,
                    timeout=30.0
                )
                response.raise_for_status()
                return response.json()
            except httpx.HTTPStatusError as e:
                print(f"Error saving report to .NET API: {e.response.status_code} - {e.response.text}")
                raise
            except Exception as e:
                print(f"Error connecting to .NET API: {e}")
                raise
    
    async def get_job_details(self, job_id: str) -> dict[str, Any]:
        """Fetch job details from .NET API"""
        async with httpx.AsyncClient() as client:
            try:
                response = await client.get(
                    f"{self.base_url}/api/jobs/{job_id}",
                    timeout=30.0
                )
                response.raise_for_status()
                return response.json()
            except Exception as e:
                print(f"Error fetching job details: {e}")
                raise


# Singleton instance
dotnet_client = DotNetClient()
