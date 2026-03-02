import json
import logging

from app.core.llm_client import llm_client
from app.schemas.cv_data import CvData
from app.schemas.evaluation import (
    SkillsAnalysis, 
    EducationAnalysis, 
    ExperienceAnalysis,
    EvaluationSummary
)


logger = logging.getLogger(__name__)


class ScoringTool:
    """Tool for scoring candidates against job requirements"""
    
    def __init__(self):
        self.llm = llm_client
    
    def analyze_skills(self, candidate_skills: list[str], required_skills: list[str]) -> SkillsAnalysis:
        logger.info(f"Analyzing skills - Candidate: {candidate_skills}, Required: {required_skills}")
        """Compare candidate skills with required skills"""
        candidate_skills_lower = {s.lower().strip() for s in candidate_skills}
        required_skills_lower = {s.lower().strip() for s in required_skills}
        
        # Find matches (including partial matches)
        matched = []
        missing = []
        
        for required in required_skills:
            req_lower = required.lower().strip()
            # Check for exact or partial match
            found = False
            for candidate in candidate_skills:
                cand_lower = candidate.lower().strip()
                if req_lower in cand_lower or cand_lower in req_lower:
                    matched.append(required)
                    found = True
                    break
            if not found:
                missing.append(required)
        
        # Calculate percentage
        total_required = len(required_skills) if required_skills else 1
        match_percentage = int((len(matched) / total_required) * 100)
        
        # Generate details using LLM
        details = self._generate_skills_details(matched, missing, candidate_skills)
        
        return SkillsAnalysis(
            matched_skills=matched,
            missing_skills=missing,
            match_percentage=match_percentage,
            details=details
        )
    
    def _generate_skills_details(self, matched: list[str], missing: list[str], 
                                  all_candidate_skills: list[str]) -> str:
        """Generate detailed skills analysis"""
        prompt = f"""Provide a brief 2-3 sentence analysis of this skills match:

Matched required skills: {matched}
Missing required skills: {missing}
All candidate skills: {all_candidate_skills[:30]}

Be specific about strengths and gaps."""

        system = "You are a technical recruiter providing skills analysis."
        
        try:
            return self.llm.generate(prompt, system, temperature=0.3, max_tokens=200)
        except Exception as e:
            return f"Matched {len(matched)} of {len(matched) + len(missing)} required skills."
    
    def analyze_education(self, candidate_education: list[str], required_education: str) -> EducationAnalysis:
        """Analyze education match"""
        if not required_education:
            return EducationAnalysis(
                required="Not specified",
                candidate=", ".join(candidate_education) if candidate_education else "Not specified",
                match=True,
                score=100
            )
        
        prompt = f"""Compare candidate education with job requirements:

Required: {required_education}
Candidate has: {candidate_education}

Return JSON with:
- "match": boolean (true if candidate meets or exceeds requirements)
- "score": integer 0-100 (how well education matches)
- "candidate_summary": brief summary of candidate's education

JSON:"""

        system = "Analyze education match and return JSON."
        
        try:
            result = self.llm.generate_json(prompt, system, temperature=0.1)
            
            return EducationAnalysis(
                required=required_education,
                candidate=result.get("candidate_summary", ", ".join(candidate_education)),
                match=result.get("match", False),
                score=result.get("score", 50)
            )
        except Exception as e:
            print(f"Education analysis error: {e}")
            return EducationAnalysis(
                required=required_education,
                candidate=", ".join(candidate_education) if candidate_education else "Unknown",
                match=False,
                score=50
            )
    
    def analyze_experience(self, candidate_experience: list[str], candidate_years: int, 
                          required_years: int) -> ExperienceAnalysis:
        """Analyze experience match"""
        # Calculate score based on years
        if required_years == 0:
            score = 100
        elif candidate_years >= required_years:
            score = 100
        else:
            score = int((candidate_years / required_years) * 100)
        
        return ExperienceAnalysis(
            required_years=required_years,
            candidate_years=candidate_years,
            relevant_experience=candidate_experience[:5],  # Top 5 experiences
            score=min(score, 100)
        )
    
    def calculate_total_score(self, skills_analysis: SkillsAnalysis,
                              education_analysis: EducationAnalysis,
                              experience_analysis: ExperienceAnalysis) -> int:
        """Calculate weighted total score"""
        # Weights: Skills 50%, Experience 30%, Education 20%
        skills_weight = 0.50
        experience_weight = 0.30
        education_weight = 0.20
        
        total = (
            skills_analysis.match_percentage * skills_weight +
            experience_analysis.score * experience_weight +
            education_analysis.score * education_weight
        )
        
        return int(total)
    
    def generate_summary(self, cv_data: CvData, job_title: str, job_requirements: str,
                        skills_analysis: SkillsAnalysis, education_analysis: EducationAnalysis,
                        experience_analysis: ExperienceAnalysis, total_score: int,
                        status: str) -> EvaluationSummary:
        """Generate evaluation summary with reasoning"""
        prompt = f"""Generate an evaluation summary for this job application:

Job: {job_title}
Requirements: {job_requirements[:500]}

Candidate:
- Skills match: {skills_analysis.match_percentage}%
- Matched skills: {skills_analysis.matched_skills}
- Missing skills: {skills_analysis.missing_skills}
- Experience: {experience_analysis.candidate_years} years (required: {experience_analysis.required_years})
- Education match: {education_analysis.match}

Total Score: {total_score}
Decision: {status}

Return JSON with:
- "decision": "{status}"
- "reasoning": 2-3 sentence explanation for the decision
- "strengths": list of 2-4 candidate strengths
- "concerns": list of 1-3 concerns or areas for improvement

JSON:"""

        system = "Generate evaluation summary as JSON."
        
        try:
            result = self.llm.generate_json(prompt, system, temperature=0.3)
            
            return EvaluationSummary(
                decision=status,
                reasoning=result.get("reasoning", f"Score of {total_score} resulted in {status} decision."),
                strengths=result.get("strengths", []),
                concerns=result.get("concerns", [])
            )
        except Exception as e:
            print(f"Summary generation error: {e}")
            return EvaluationSummary(
                decision=status,
                reasoning=f"Based on the evaluation score of {total_score}, the candidate is {status.lower()}.",
                strengths=["Unable to analyze strengths"],
                concerns=["Unable to analyze concerns"]
            )


# Singleton instance
scoring_tool = ScoringTool()
