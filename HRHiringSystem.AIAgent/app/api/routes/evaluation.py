from fastapi import APIRouter, HTTPException
import logging
import traceback

from app.schemas.evaluation import EvaluationRequest, EvaluationResponse
from app.agents.hiring_agent import hiring_agent
from app.services.blob_service import blob_service
from app.services.dotnet_client import dotnet_client

router = APIRouter(prefix="/api", tags=["AI Evaluation"])
logger = logging.getLogger(__name__)


@router.post("/evaluate", response_model=EvaluationResponse)
async def evaluate_application(request: EvaluationRequest):
    """
    Perform full AI evaluation of a job application.
    
    This endpoint:
    1. Downloads CV from blob storage
    2. Extracts skills, education, experience
    3. Compares against job requirements
    4. Calculates scores
    5. Generates interview questions (if score >= 65)
    6. Self-reflects and validates results
    7. Saves report to .NET API (MongoDB + SQL update)
    
    Returns full evaluation report.
    """
    try:
        logger.info(f"Starting evaluation for application {request.application_id}")
        
        # Step 1: Download CV from blob storage
        logger.info(f"Downloading CV from {request.cv_url}")
        cv_content, filename = await blob_service.download_cv(request.cv_url)
        logger.info(f"Downloaded CV: {filename}, {len(cv_content)} bytes")
        
        # Step 2: Run AI evaluation
        logger.info("Starting AI evaluation")
        evaluation = await hiring_agent.evaluate(request, cv_content, filename)
        logger.info(f"Evaluation complete. Score: {evaluation.score}")
        
        # Step 3: Save report to .NET API
        try:
            await dotnet_client.save_evaluation_report(request, evaluation)
        except Exception as e:
            # Log but don't fail - evaluation is complete
            logger.warning(f"Failed to save report to .NET API: {e}")
        
        return evaluation
        
    except RuntimeError as e:
        logger.error(f"Runtime error: {e}\n{traceback.format_exc()}")
        raise HTTPException(status_code=500, detail=str(e))
    except Exception as e:
        logger.error(f"Evaluation failed: {e}\n{traceback.format_exc()}")
        raise HTTPException(
            status_code=500,
            detail=f"Evaluation failed: {str(e)}"
        )


@router.post("/evaluate/dry-run", response_model=EvaluationResponse)
async def evaluate_dry_run(request: EvaluationRequest):
    """
    Perform AI evaluation without saving to database.
    Useful for testing and debugging.
    """
    try:
        cv_content, filename = await blob_service.download_cv(request.cv_url)
        evaluation = await hiring_agent.evaluate(request, cv_content, filename)
        return evaluation
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Evaluation failed: {str(e)}"
        )
