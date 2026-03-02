import json
import time
from typing import Optional

from app.core.config import get_settings
from app.schemas.cv_data import CvData
from app.schemas.evaluation import (
    EvaluationRequest,
    EvaluationResponse,
    AIMetadata
)
from app.tools.cv_parser import cv_parser
from app.tools.skill_extractor import skill_extractor
from app.tools.scoring_tool import scoring_tool
from app.tools.question_generator import question_generator
from app.agents.reflection import reflection_tool


class HiringAgent:
    """
    Main AI Agent that orchestrates the hiring evaluation process.
    
    Workflow:
    1. Parse CV text from blob storage
    2. Extract skills, education, and experience
    3. Compare against job requirements
    4. Calculate scores
    5. Generate interview questions (if score >= 65)
    6. Self-reflect and validate
    7. Generate final evaluation report
    """
    
    def __init__(self):
        self.settings = get_settings()
    
    async def evaluate(self, request: EvaluationRequest, cv_content: bytes, 
                       filename: str) -> EvaluationResponse:
        """
        Perform full evaluation of a job application.
        """
        start_time = time.time()
        
        # Step 1: Parse CV
        cv_data = cv_parser.parse(cv_content, filename)
        
        # Step 2: Enrich CV data with skills, education, experience
        cv_data = skill_extractor.enrich_cv_data(cv_data)
        
        # Step 3: Parse required skills from JSON or extract from job requirements
        required_skills = self._parse_required_skills(request.job_required_skills)
        
        # If no explicit skills were provided, extract from job requirements text
        if not required_skills and request.job_requirements:
            required_skills = skill_extractor.extract_skills_from_job(request.job_requirements)
        
        # Step 4: Analyze skills match
        skills_analysis = scoring_tool.analyze_skills(
            candidate_skills=cv_data.skills,
            required_skills=required_skills
        )
        
        # Step 5: Analyze education
        education_analysis = scoring_tool.analyze_education(
            candidate_education=cv_data.education,
            required_education=request.job_education or ""
        )
        
        # Step 6: Analyze experience
        experience_analysis = scoring_tool.analyze_experience(
            candidate_experience=cv_data.experience,
            candidate_years=cv_data.years_of_experience,
            required_years=request.job_experience_years
        )
        
        # Step 7: Calculate total score
        total_score = scoring_tool.calculate_total_score(
            skills_analysis=skills_analysis,
            education_analysis=education_analysis,
            experience_analysis=experience_analysis
        )
        
        # Step 8: Determine status
        status = self._determine_status(total_score)
        
        # Step 9: Generate summary
        summary = scoring_tool.generate_summary(
            cv_data=cv_data,
            job_title=request.job_title,
            job_requirements=request.job_requirements,
            skills_analysis=skills_analysis,
            education_analysis=education_analysis,
            experience_analysis=experience_analysis,
            total_score=total_score,
            status=status
        )
        
        # Step 10: Generate interview questions (if not rejected)
        interview_questions = []
        if total_score >= self.settings.score_hr_review_threshold:
            interview_questions = question_generator.generate_questions(
                job_title=request.job_title,
                job_requirements=request.job_requirements,
                required_skills=required_skills,
                candidate_skills=cv_data.skills,
                cv_data=cv_data,
                num_questions=20
            )
        
        # Build initial evaluation
        evaluation = EvaluationResponse(
            score=total_score,
            status=status,
            skills_analysis=skills_analysis,
            education_analysis=education_analysis,
            experience_analysis=experience_analysis,
            summary=summary,
            interview_questions=interview_questions,
            ai_metadata=AIMetadata(
                model=self.settings.gemini_model,
                processing_time_ms=0,
                reflection_iterations=0
            )
        )
        
        # Step 11: Self-reflection and validation
        evaluation, reflection_iterations = reflection_tool.reflect_and_validate(
            evaluation=evaluation,
            cv_text=cv_data.raw_text,
            job_requirements=request.job_requirements
        )
        
        # Step 12: Generate interview questions after reflection if needed
        # (in case reflection upgraded status from Rejected to Accepted/HRReview)
        if (evaluation.score >= self.settings.score_hr_review_threshold and 
            len(evaluation.interview_questions) == 0):
            evaluation.interview_questions = question_generator.generate_questions(
                job_title=request.job_title,
                job_requirements=request.job_requirements,
                required_skills=required_skills,
                candidate_skills=cv_data.skills,
                cv_data=cv_data,
                num_questions=20
            )
        
        # Update metadata
        processing_time_ms = int((time.time() - start_time) * 1000)
        evaluation.ai_metadata.processing_time_ms = processing_time_ms
        evaluation.ai_metadata.reflection_iterations = reflection_iterations
        
        return evaluation
    
    def _parse_required_skills(self, skills_json: str) -> list[str]:
        """Parse required skills from JSON string"""
        try:
            skills = json.loads(skills_json)
            if isinstance(skills, list):
                return [str(s).strip() for s in skills if s]
        except json.JSONDecodeError:
            # Try comma-separated fallback
            return [s.strip() for s in skills_json.split(',') if s.strip()]
        return []
    
    def _determine_status(self, score: int) -> str:
        """Determine application status based on score"""
        if score >= self.settings.score_accepted_threshold:
            return "Accepted"
        elif score >= self.settings.score_hr_review_threshold:
            return "HRReview"
        else:
            return "Rejected"


# Singleton instance
hiring_agent = HiringAgent()
