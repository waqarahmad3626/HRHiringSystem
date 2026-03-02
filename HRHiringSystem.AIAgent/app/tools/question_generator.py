import json
import logging

from app.core.llm_client import llm_client
from app.schemas.evaluation import InterviewQuestion
from app.schemas.cv_data import CvData

logger = logging.getLogger(__name__)


class QuestionGenerator:
    """Tool for generating interview questions based on job and candidate profile"""
    
    def __init__(self):
        self.llm = llm_client
    
    def generate_questions(self, job_title: str, job_requirements: str, 
                          required_skills: list[str], candidate_skills: list[str],
                          cv_data: CvData, num_questions: int = 20) -> list[InterviewQuestion]:
        """Generate interview questions tailored to the job and candidate"""
        
        prompt = f"""Generate {num_questions} interview questions for a {job_title} position.

Job Requirements:
{job_requirements[:1000]}

Required Skills: {required_skills}
Candidate Skills: {candidate_skills}
Candidate Experience: {cv_data.years_of_experience} years
Candidate Background: {cv_data.experience[:3]}

Generate a balanced set of questions covering:
1. Technical skills (related to job requirements)
2. Architecture and system design
3. SOLID principles and design patterns
4. Problem-solving and coding
5. Behavioral/soft skills

IMPORTANT: For EVERY question, you MUST include an "expectedAnswer" field that describes what a good candidate should cover in their response. The expectedAnswer should be 2-3 sentences explaining the key points to look for.

Return a JSON array with EXACTLY this structure (all 4 fields are required for each question):
[
  {{"category": "Technical - C#", "question": "Explain async/await in C#.", "difficulty": "Medium", "expectedAnswer": "A good answer should explain that async/await enables non-blocking I/O operations, improves application responsiveness, and discuss Task-based patterns."}},
  {{"category": "Architecture", "question": "How do you design for scalability?", "difficulty": "Hard", "expectedAnswer": "Key points: horizontal scaling, load balancing, caching strategies, database sharding, and stateless services."}},
  ...
]

Difficulty levels: Easy, Medium, Hard

JSON array (include expectedAnswer for every question):

JSON array:"""

        system = "You are a senior technical interviewer. Generate comprehensive interview questions as JSON. EVERY question MUST include an expectedAnswer field with 2-3 sentences describing what to look for in a good response."
        
        try:
            questions_data = self.llm.generate_json(prompt, system, temperature=0.5)
            
            # Debug: Log the raw response from LLM
            if questions_data and len(questions_data) > 0:
                logger.info(f"LLM returned {len(questions_data)} questions. First question keys: {questions_data[0].keys() if isinstance(questions_data[0], dict) else 'not a dict'}")
                if isinstance(questions_data[0], dict) and 'expectedAnswer' in questions_data[0]:
                    logger.info(f"First expectedAnswer: {questions_data[0].get('expectedAnswer', 'MISSING')[:100]}...")
                else:
                    logger.warning("LLM did not include expectedAnswer in questions!")
            
            questions = []
            for q in questions_data:
                if isinstance(q, dict):
                    questions.append(InterviewQuestion(
                        category=q.get("category", "General"),
                        question=q.get("question", ""),
                        difficulty=q.get("difficulty", "Medium"),
                        expected_answer=q.get("expectedAnswer", "")
                    ))
            
            return questions[:num_questions]
            
        except Exception as e:
            print(f"Question generation error: {e}")
            return self._generate_fallback_questions(job_title, required_skills)
    
    def _generate_fallback_questions(self, job_title: str, required_skills: list[str]) -> list[InterviewQuestion]:
        """Generate basic fallback questions if LLM fails"""
        questions = [
            InterviewQuestion(
                category="General",
                question=f"Tell me about your experience relevant to the {job_title} role.",
                difficulty="Easy",
                expected_answer="Look for relevant projects, technologies used, and specific achievements that align with the job requirements."
            ),
            InterviewQuestion(
                category="Technical",
                question=f"What experience do you have with {required_skills[0] if required_skills else 'the required technologies'}?",
                difficulty="Medium",
                expected_answer="Candidate should describe specific projects, depth of usage, and practical challenges overcome with the technology."
            ),
            InterviewQuestion(
                category="Problem Solving",
                question="Describe a challenging technical problem you solved recently.",
                difficulty="Medium",
                expected_answer="Look for clear problem definition, systematic approach, consideration of alternatives, and measurable outcome."
            ),
            InterviewQuestion(
                category="Architecture",
                question="How would you design a scalable system for handling millions of requests?",
                difficulty="Hard",
                expected_answer="Should mention load balancing, caching strategies, database sharding/replication, microservices, async processing, and monitoring."
            ),
            InterviewQuestion(
                category="SOLID Principles",
                question="Explain the Single Responsibility Principle and give an example.",
                difficulty="Medium",
                expected_answer="A class should have only one reason to change. Example: separating data access from business logic."
            ),
            InterviewQuestion(
                category="Design Patterns",
                question="What design patterns have you used and when would you apply them?",
                difficulty="Medium",
                expected_answer="Should mention patterns like Factory, Repository, Strategy, or Observer with practical use cases and benefits."
            ),
            InterviewQuestion(
                category="Behavioral",
                question="Describe a time when you had to work under pressure to meet a deadline.",
                difficulty="Easy",
                expected_answer="Look for prioritization skills, communication with stakeholders, and ability to deliver quality work under constraints."
            ),
            InterviewQuestion(
                category="Team Collaboration",
                question="How do you handle disagreements with team members about technical decisions?",
                difficulty="Easy",
                expected_answer="Should demonstrate active listening, data-driven arguments, willingness to compromise, and focus on team goals."
            ),
        ]
        return questions


# Singleton instance
question_generator = QuestionGenerator()
