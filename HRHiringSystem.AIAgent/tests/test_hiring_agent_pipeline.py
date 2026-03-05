import asyncio

from app.agents.hiring_agent import HiringAgent
from app.schemas.cv_data import CvData
from app.schemas.evaluation import (
    EducationAnalysis,
    EvaluationRequest,
    EvaluationSummary,
    ExperienceAnalysis,
    SkillsAnalysis,
)


def test_evaluate_pipeline_runs_with_rag_and_adaptive_path() -> None:
    agent = HiringAgent()

    sample_cv = CvData(
        raw_text="Python C# SQL Docker Kubernetes with 6 years experience",
        education=["BS Computer Science"],
        experience=["Senior Engineer at ExampleCorp"],
        years_of_experience=6,
    )

    from app.agents import hiring_agent as module

    module.cv_parser.parse = lambda content, filename: sample_cv
    module.skill_extractor.extract_education = lambda text: ["BS Computer Science"]
    module.skill_extractor.extract_work_experience = lambda text: ["Senior Engineer at ExampleCorp"]
    module.skill_extractor.extract_experience_years = lambda text: 6

    module.scoring_tool.analyze_skills = lambda candidate_skills, required_skills: SkillsAnalysis(
        matchedSkills=required_skills,
        missingSkills=[],
        matchPercentage=100,
        details="Strong overlap",
    )
    module.scoring_tool.analyze_education = lambda candidate_education, required_education: EducationAnalysis(
        required=required_education,
        candidate="BS Computer Science",
        match=True,
        score=90,
    )
    module.scoring_tool.analyze_experience = lambda candidate_experience, candidate_years, required_years: ExperienceAnalysis(
        requiredYears=required_years,
        candidateYears=candidate_years,
        relevantExperience=candidate_experience,
        score=90,
    )
    module.scoring_tool.calculate_total_score = lambda skills_analysis, education_analysis, experience_analysis: 92
    module.scoring_tool.generate_summary = lambda **kwargs: EvaluationSummary(
        decision="Accepted",
        reasoning="Candidate meets core requirements.",
        strengths=["Strong backend skills"],
        concerns=[],
    )

    module.question_generator.generate_questions = lambda **kwargs: []
    module.reflection_tool.reflect_and_validate = lambda evaluation, cv_text, job_requirements: (evaluation, 1)

    rag_calls = {"indexed": False}
    module.rag_service.build_context = lambda **kwargs: "retrieved context"

    def fake_index(**kwargs):
        rag_calls["indexed"] = True

    module.rag_service.index_evaluation_report = fake_index

    request = EvaluationRequest(
        candidateId="cand-1",
        jobId="job-1",
        applicationId="app-1",
        cvUrl="https://example.com/cv.pdf",
        jobTitle="Senior Backend Engineer",
        jobDescription="Backend platform role",
        jobRequirements="Python, C#, SQL, Docker",
        jobRequiredSkills='["Python", "C#", "SQL", "Docker"]',
        jobExperienceYears=4,
        jobEducation="BS in Computer Science",
    )

    result = asyncio.run(agent.evaluate(request, b"fake pdf", "resume.pdf"))

    assert result.score == 92
    assert result.status == "Accepted"
    assert rag_calls["indexed"] is True
