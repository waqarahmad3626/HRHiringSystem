from app.agents.reflection import ReflectionTool
from app.schemas.evaluation import (
    EducationAnalysis,
    EvaluationResponse,
    EvaluationSummary,
    ExperienceAnalysis,
    SkillsAnalysis,
)


def test_reflection_updates_score_and_status_when_invalid() -> None:
    tool = ReflectionTool()

    evaluation = EvaluationResponse(
        score=60,
        status="Rejected",
        skillsAnalysis=SkillsAnalysis(matchPercentage=60),
        educationAnalysis=EducationAnalysis(score=60),
        experienceAnalysis=ExperienceAnalysis(score=60),
        summary=EvaluationSummary(decision="Rejected", reasoning="Initial reasoning"),
    )

    calls = {"count": 0}

    def fake_reflect_once(current_evaluation, cv_text, job_requirements):
        calls["count"] += 1
        if calls["count"] == 1:
            return {
                "is_valid": False,
                "suggested_score": 82,
                "reasoning_adjustment": "Underestimated transferable experience",
            }
        return {"is_valid": True, "reasoning_adjustment": "No issues found"}

    tool._reflect_once = fake_reflect_once

    updated, iterations = tool.reflect_and_validate(
        evaluation=evaluation,
        cv_text="Sample CV text",
        job_requirements="Sample requirements",
        max_iterations=2,
    )

    assert updated.score == 82
    assert updated.status == "Accepted"
    assert updated.summary.decision == "Accepted"
    assert iterations == 2
