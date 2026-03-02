import json
import logging
from typing import Optional

from app.core.llm_client import llm_client
from app.schemas.cv_data import CvData

logger = logging.getLogger(__name__)


class SkillExtractor:
    """Tool for extracting skills from CV text using LLM"""
    
    def __init__(self):
        self.llm = llm_client
    
    def extract_skills(self, cv_text: str) -> list[str]:
        """Extract technical and soft skills from CV text"""
        prompt = f"""Analyze the following CV/resume text and extract all technical skills, 
programming languages, frameworks, tools, and relevant soft skills.

Return ONLY a JSON array of skill strings, nothing else.
Example: ["Python", "JavaScript", "React", "SQL", "Team Leadership"]

CV Text:
{cv_text[:8000]}

Skills JSON array:"""

        system = "You are a technical recruiter analyzing resumes. Extract skills as a JSON array."
        
        try:
            logger.info(f"Extracting skills from CV text ({len(cv_text)} chars)")
            skills = self.llm.generate_json(prompt, system, temperature=0.1)
            logger.info(f"LLM returned skills: {skills}")
            if isinstance(skills, list):
                result = [str(s).strip() for s in skills if s]
                logger.info(f"Extracted {len(result)} skills: {result[:10]}...")
                return result
            else:
                logger.warning(f"LLM returned non-list: {type(skills)}")
        except Exception as e:
            logger.error(f"Skill extraction error: {e}")
        
        return []
    
    def extract_experience_years(self, cv_text: str) -> int:
        """Extract total years of experience from CV"""
        prompt = f"""Analyze the following CV/resume and determine the total years of professional work experience.
Consider the work history section and calculate the approximate total years.

Return ONLY a single integer number representing years of experience.

CV Text:
{cv_text[:8000]}

Years of experience (integer only):"""

        system = "You are analyzing work experience. Return only an integer."
        
        try:
            content = self.llm.generate(prompt, system, temperature=0.1, max_tokens=10)
            return int(content.strip())
        except Exception as e:
            print(f"Experience extraction error: {e}")
        
        return 0
    
    def extract_education(self, cv_text: str) -> list[str]:
        """Extract education details from CV"""
        prompt = f"""Analyze the following CV/resume and extract education information.
Include degree type, field of study, and institution name.

Return ONLY a JSON array of education strings.
Example: ["BS Computer Science - MIT", "MS Data Science - Stanford"]

CV Text:
{cv_text[:8000]}

Education JSON array:"""

        system = "Extract education as a JSON array."
        
        try:
            education = self.llm.generate_json(prompt, system, temperature=0.1)
            if isinstance(education, list):
                return [str(e).strip() for e in education if e]
        except Exception as e:
            print(f"Education extraction error: {e}")
        
        return []
    
    def extract_work_experience(self, cv_text: str) -> list[str]:
        """Extract work experience entries from CV"""
        prompt = f"""Analyze the following CV/resume and extract work experience entries.
Include job title and company name for each position.

Return ONLY a JSON array of work experience strings.
Example: ["Senior Developer at Google", "Software Engineer at Microsoft"]

CV Text:
{cv_text[:8000]}

Work experience JSON array:"""

        system = "Extract work experience as a JSON array."
        
        try:
            experience = self.llm.generate_json(prompt, system, temperature=0.1)
            if isinstance(experience, list):
                return [str(e).strip() for e in experience if e]
        except Exception as e:
            print(f"Work experience extraction error: {e}")
        
        return []
    
    def extract_skills_from_job(self, job_requirements: str) -> list[str]:
        """Extract required skills from job requirements text"""
        prompt = f"""Analyze the following job requirements and extract all required technical skills,
programming languages, frameworks, tools, and technologies.

Return ONLY a JSON array of skill strings, nothing else.
Example: ["Python", "JavaScript", "AWS", "Docker", "SQL"]

Job Requirements:
{job_requirements[:5000]}

Required Skills JSON array:"""

        system = "You are a technical recruiter analyzing job postings. Extract required skills as a JSON array."
        
        try:
            logger.info(f"Extracting skills from job requirements ({len(job_requirements)} chars)")
            skills = self.llm.generate_json(prompt, system, temperature=0.1)
            logger.info(f"Extracted job skills: {skills}")
            if isinstance(skills, list):
                result = [str(s).strip() for s in skills if s]
                logger.info(f"Extracted {len(result)} job skills: {result}")
                return result
        except Exception as e:
            logger.error(f"Job skills extraction error: {e}")
        
        return []

    def enrich_cv_data(self, cv_data: CvData) -> CvData:
        """Enrich CV data with extracted skills, education, and experience"""
        logger.info(f"Enriching CV data. Raw text length: {len(cv_data.raw_text)}")
        cv_data.skills = self.extract_skills(cv_data.raw_text)
        logger.info(f"Extracted skills: {cv_data.skills}")
        cv_data.education = self.extract_education(cv_data.raw_text)
        logger.info(f"Extracted education: {cv_data.education}")
        cv_data.experience = self.extract_work_experience(cv_data.raw_text)
        logger.info(f"Extracted experience: {cv_data.experience}")
        cv_data.years_of_experience = self.extract_experience_years(cv_data.raw_text)
        logger.info(f"Extracted years: {cv_data.years_of_experience}")
        return cv_data


# Singleton instance
skill_extractor = SkillExtractor()
