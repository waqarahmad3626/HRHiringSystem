from app.schemas.evaluation import EducationAnalysis, ExperienceAnalysis, SkillsAnalysis
from app.tools.scoring_tool import ScoringTool


def test_calculate_total_score_uses_50_20_30_weights() -> None:
    tool = ScoringTool()
    total = tool.calculate_total_score(
        skills_analysis=SkillsAnalysis(matchPercentage=80),
        education_analysis=EducationAnalysis(score=50),
        experience_analysis=ExperienceAnalysis(score=70),
    )
    assert total == 71


def test_analyze_experience_caps_at_100() -> None:
    tool = ScoringTool()
    result = tool.analyze_experience(
        candidate_experience=["Engineer at X"],
        candidate_years=10,
        required_years=4,
    )
    assert result.score == 100


def test_analyze_skills_match_percentage_without_llm() -> None:
    tool = ScoringTool()
    tool._generate_skills_details = lambda matched, missing, all_candidate_skills: "ok"

    result = tool.analyze_skills(
        candidate_skills=["Python", "C#", "SQL Server"],
        required_skills=["Python", "SQL", "Docker"],
    )

    assert result.match_percentage == 66
    assert "Python" in result.matched_skills
    assert "SQL" in result.matched_skills
    assert "Docker" in result.missing_skills
