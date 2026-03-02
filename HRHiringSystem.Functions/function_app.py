import azure.functions as func
import logging
import httpx
import json
import os

app = func.FunctionApp()

# Shared API key for authentication (only .NET API and AI Agent should know this)
FUNCTION_API_KEY = os.environ.get('FUNCTION_API_KEY', 'hr-hiring-function-secret-key-2026')


def validate_api_key(req: func.HttpRequest) -> bool:
    """Validate the API key from the request header."""
    api_key = req.headers.get('X-Function-Key') or req.headers.get('x-function-key')
    return api_key == FUNCTION_API_KEY


@app.function_name(name="ProcessJobApplication")
@app.route(route="process-application", methods=["POST"], auth_level=func.AuthLevel.ANONYMOUS)
async def process_job_application(req: func.HttpRequest) -> func.HttpResponse:
    """
    HTTP-triggered function to process a job application through the AI Agent.
    
    Expected request body:
    {
        "candidateId": "guid",
        "jobId": "guid",
        "applicationId": "guid",
        "cvUrl": "blob-url"
    }
    
    Required header:
    X-Function-Key: <shared-api-key>
    
    This function:
    1. Fetches job details from .NET API
    2. Calls AI Agent for evaluation
    3. Returns evaluation result (AI Agent saves to MongoDB via .NET API)
    """
    logging.info('ProcessJobApplication function triggered')
    
    # Log request details for debugging
    logging.info(f'Request method: {req.method}')
    logging.info(f'Request headers: {dict(req.headers)}')
    try:
        body_bytes = req.get_body()
        logging.info(f'Request body bytes length: {len(body_bytes)}')
        logging.info(f'Request body: {body_bytes.decode("utf-8")[:500]}')
    except Exception as e:
        logging.error(f'Error reading body: {e}')
    
    # Validate API key
    if not validate_api_key(req):
        logging.warning('Unauthorized request - invalid or missing API key')
        return func.HttpResponse(
            json.dumps({"error": "Unauthorized - invalid or missing API key"}),
            status_code=401,
            mimetype="application/json"
        )
    
    try:
        # Parse request - handle empty body or invalid JSON
        try:
            req_body = req.get_json()
        except ValueError:
            # get_json throws ValueError when body is empty or invalid
            req_body = None
            
        if not req_body:
            logging.error('Empty or invalid request body')
            return func.HttpResponse(
                json.dumps({"error": "Request body is empty or invalid JSON"}),
                status_code=400,
                mimetype="application/json"
            )
        
        candidate_id = req_body.get('candidateId')
        job_id = req_body.get('jobId')
        application_id = req_body.get('applicationId')
        cv_url = req_body.get('cvUrl')
        
        if not all([candidate_id, job_id, application_id, cv_url]):
            return func.HttpResponse(
                json.dumps({"error": "Missing required fields"}),
                status_code=400,
                mimetype="application/json"
            )
        
        # Get configuration
        ai_agent_url = os.environ.get('AI_AGENT_URL', 'http://localhost:8001')
        dotnet_api_url = os.environ.get('DOTNET_API_URL', 'http://localhost:8080')
        
        async with httpx.AsyncClient(timeout=300.0) as client:
            # Step 1: Fetch job details from .NET API
            logging.info(f'Fetching job details for job {job_id}')
            job_response = await client.get(f"{dotnet_api_url}/api/jobs/{job_id}")
            
            if job_response.status_code != 200:
                logging.error(f'Failed to fetch job: {job_response.status_code}')
                return func.HttpResponse(
                    json.dumps({"error": "Failed to fetch job details"}),
                    status_code=500,
                    mimetype="application/json"
                )
            
            job_data = job_response.json().get('data', {})
            
            # Step 2: Build evaluation request
            evaluation_request = {
                "candidateId": candidate_id,
                "jobId": job_id,
                "applicationId": application_id,
                "cvUrl": cv_url,
                "jobTitle": job_data.get('jobTitle', ''),
                "jobDescription": job_data.get('jobDescription', ''),
                "jobRequirements": job_data.get('jobRequirements', ''),
                "jobRequiredSkills": job_data.get('jobRequiredSkills', '[]'),
                "jobExperienceYears": job_data.get('jobExperienceYears', 0),
                "jobEducation": job_data.get('jobEducation')
            }
            
            # Step 3: Call AI Agent for evaluation
            logging.info(f'Calling AI Agent for evaluation of application {application_id}')
            ai_response = await client.post(
                f"{ai_agent_url}/api/evaluate",
                json=evaluation_request
            )
            
            if ai_response.status_code != 200:
                logging.error(f'AI Agent evaluation failed: {ai_response.status_code}')
                return func.HttpResponse(
                    json.dumps({
                        "error": "AI evaluation failed",
                        "details": ai_response.text
                    }),
                    status_code=500,
                    mimetype="application/json"
                )
            
            result = ai_response.json()
            logging.info(f'Evaluation complete. Score: {result.get("score")}, Status: {result.get("status")}')
            
            return func.HttpResponse(
                json.dumps({
                    "success": True,
                    "applicationId": application_id,
                    "score": result.get("score"),
                    "status": result.get("status")
                }),
                status_code=200,
                mimetype="application/json"
            )
            
    except json.JSONDecodeError:
        return func.HttpResponse(
            json.dumps({"error": "Invalid JSON in request body"}),
            status_code=400,
            mimetype="application/json"
        )
    except Exception as e:
        logging.error(f'Function error: {str(e)}')
        return func.HttpResponse(
            json.dumps({"error": str(e)}),
            status_code=500,
            mimetype="application/json"
        )
