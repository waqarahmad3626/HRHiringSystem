export interface EvaluationReport {
  id: string;
  candidateId: string;
  jobId: string;
  applicationId: string;
  overallScore: number;
  status: string;
  skillsAnalysis: SkillsAnalysis;
  educationAnalysis: EducationAnalysis;
  experienceAnalysis: ExperienceAnalysis;
  summary: EvaluationSummary;
  interviewQuestions: InterviewQuestion[];
  aiMetadata: AIMetadata;
  createdAt: Date;
}

export interface SkillsAnalysis {
  matchedSkills: string[];
  missingSkills: string[];
  additionalSkills: string[];
  skillMatchPercentage: number;
  skillsScore: number;
  details?: string;
}

export interface EducationAnalysis {
  candidateEducation: string;
  requiredEducation: string;
  educationMet: boolean;
  educationScore: number;
  educationComments: string;
}

export interface ExperienceAnalysis {
  candidateYears: number;
  requiredYears: number;
  experienceMet: boolean;
  experienceScore: number;
  experienceComments: string;
}

export interface EvaluationSummary {
  strengths: string[];
  weaknesses: string[];
  recommendation: string;
  cultureFitAssessment: string;
  decision?: string;
  reasoning?: string;
}

export interface InterviewQuestion {
  question: string;
  category: string;
  difficulty: string;
  expectedAnswer: string;
}

export interface AIMetadata {
  modelUsed: string;
  parsedAt: Date;
  evaluatedAt: Date;
  reflectionApplied: boolean;
  totalProcessingTimeMs: number;
}

export interface CvParseResponse {
  success: boolean;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  skills: string[];
  education?: string;
  yearsOfExperience?: number;
  rawText?: string;
  error?: string;
}
