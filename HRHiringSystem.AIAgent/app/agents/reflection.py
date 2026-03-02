import json

from app.core.config import get_settings
from app.core.llm_client import llm_client
from app.schemas.evaluation import EvaluationResponse


class ReflectionTool:
    """Tool for AI self-reflection and validation of evaluation results"""
    
    def __init__(self):
        self.llm = llm_client
    
    def reflect_and_validate(self, evaluation: EvaluationResponse, 
                             cv_text: str, job_requirements: str,
                             max_iterations: int = 2) -> tuple[EvaluationResponse, int]:
        """
        Reflect on the evaluation and validate/adjust if needed.
        Returns the (potentially adjusted) evaluation and number of reflection iterations.
        """
        iterations = 0
        current_evaluation = evaluation
        
        for i in range(max_iterations):
            iterations += 1
            
            reflection_result = self._reflect_once(current_evaluation, cv_text, job_requirements)
            
            if reflection_result["is_valid"]:
                # Evaluation looks good, no changes needed
                break
            
            # Apply suggested adjustments
            if "suggested_score" in reflection_result and reflection_result["suggested_score"] is not None:
                current_evaluation.score = reflection_result["suggested_score"]
                # Update status based on new score
                settings = get_settings()
                if current_evaluation.score >= settings.score_accepted_threshold:
                    current_evaluation.status = "Accepted"
                    current_evaluation.summary.decision = "Accepted"
                elif current_evaluation.score >= settings.score_hr_review_threshold:
                    current_evaluation.status = "HRReview"
                    current_evaluation.summary.decision = "HRReview"
                else:
                    current_evaluation.status = "Rejected"
                    current_evaluation.summary.decision = "Rejected"
            
            # Update summary with reflection insights
            if "reasoning_adjustment" in reflection_result:
                current_evaluation.summary.reasoning = (
                    f"{current_evaluation.summary.reasoning} "
                    f"[Adjusted after reflection: {reflection_result['reasoning_adjustment']}]"
                )
        
        return current_evaluation, iterations
    
    def _reflect_once(self, evaluation: EvaluationResponse, 
                      cv_text: str, job_requirements: str) -> dict:
        """Perform one reflection iteration"""
        prompt = f"""You are an AI evaluator reviewing your own assessment. Critically analyze this evaluation:

=== EVALUATION RESULTS ===
Score: {evaluation.score}/100
Status: {evaluation.status}
Skills Match: {evaluation.skills_analysis.match_percentage}%
Matched Skills: {evaluation.skills_analysis.matched_skills}
Missing Skills: {evaluation.skills_analysis.missing_skills}
Education Score: {evaluation.education_analysis.score}
Experience Score: {evaluation.experience_analysis.score}
Reasoning: {evaluation.summary.reasoning}

=== JOB REQUIREMENTS (excerpt) ===
{job_requirements[:1000]}

=== CV TEXT (excerpt) ===
{cv_text[:2000]}

=== REFLECTION TASK ===
1. Is the score calculation accurate given the skills match, education, and experience?
2. Does the status (Accepted/HRReview/Rejected) correctly reflect the score thresholds?
   - Score >= 80: Accepted
   - Score 65-79: HRReview
   - Score < 65: Rejected
3. Is the reasoning logical and well-justified?
4. Are there any obvious errors or inconsistencies?

Return JSON:
{{
  "is_valid": true/false,
  "issues_found": ["list of issues if any"],
  "suggested_score": null or integer (only if score should change),
  "reasoning_adjustment": "explanation of any issues or 'No issues found'"
}}

JSON:"""

        system = "You are a quality assurance reviewer. Be critical and thorough."
        
        try:
            result = self.llm.generate_json(prompt, system, temperature=0.2)
            return result
            
        except Exception as e:
            print(f"Reflection error: {e}")
            return {"is_valid": True, "reasoning_adjustment": "Reflection skipped due to error"}


# Singleton instance
reflection_tool = ReflectionTool()
