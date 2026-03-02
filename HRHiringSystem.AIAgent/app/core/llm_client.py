"""
LLM Client module - provides a unified interface for language model calls.
Currently uses Google Gemini API.
"""
import json
import logging
import time
from typing import Optional
import google.generativeai as genai
from google.api_core import exceptions as google_exceptions

from app.core.config import get_settings

logger = logging.getLogger(__name__)


class LLMClient:
    """
    Unified LLM client using Google Gemini API.
    Provides methods for generating text completions.
    """
    
    _instance: Optional["LLMClient"] = None
    _initialized: bool = False
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def __init__(self):
        if not self._initialized:
            settings = get_settings()
            if not settings.gemini_api_key:
                logger.error("GEMINI_API_KEY is not configured!")
                raise RuntimeError("GEMINI_API_KEY environment variable is not set")
            
            logger.info(f"Configuring Gemini API with model: {settings.gemini_model}")
            genai.configure(api_key=settings.gemini_api_key)
            self.model = genai.GenerativeModel(settings.gemini_model)
            self.generation_config = genai.GenerationConfig(
                temperature=settings.gemini_temperature,
                max_output_tokens=8192,
            )
            LLMClient._initialized = True
            logger.info("LLM Client initialized successfully")
    
    def generate(self, prompt: str, system_prompt: str = "", 
                 temperature: Optional[float] = None,
                 max_tokens: Optional[int] = None,
                 max_retries: int = 5) -> str:
        """
        Generate text using Gemini with retry logic for rate limits.
        
        Args:
            prompt: The user prompt
            system_prompt: Optional system instruction
            temperature: Override default temperature
            max_tokens: Override default max tokens
            max_retries: Maximum retry attempts for rate limits
            
        Returns:
            Generated text string
        """
        # Build config with overrides
        config = genai.GenerationConfig(
            temperature=temperature if temperature is not None else self.generation_config.temperature,
            max_output_tokens=max_tokens if max_tokens is not None else 8192,
        )
        
        # Combine system prompt and user prompt
        full_prompt = f"{system_prompt}\n\n{prompt}" if system_prompt else prompt
        
        last_exception = None
        for attempt in range(max_retries):
            try:
                response = self.model.generate_content(
                    full_prompt,
                    generation_config=config,
                )
                
                result = response.text.strip()
                # Rate limit: 30 RPM = wait 2 seconds between calls
                time.sleep(2)
                return result
            except Exception as e:
                error_str = str(e)
                last_exception = e
                
                # Check for 404 model not found - don't retry
                if "404" in error_str or "not found" in error_str.lower():
                    logger.error(f"Model not found error: {e}")
                    raise
                
                # Check for rate limit error (429)
                if "429" in error_str or "quota" in error_str.lower() or "rate" in error_str.lower():
                    # Wait 5 seconds before retry (30 RPM allows faster retries)
                    wait_time = 5
                    logger.warning(f"Rate limit hit, waiting {wait_time}s before retry {attempt + 1}/{max_retries}")
                    time.sleep(wait_time)
                else:
                    # For other errors, log and raise immediately
                    logger.error(f"LLM generation error: {e}")
                    raise
        
        # All retries exhausted
        logger.error(f"All {max_retries} retries exhausted for LLM generation")
        raise last_exception
    
    def generate_json(self, prompt: str, system_prompt: str = "",
                      temperature: Optional[float] = None) -> dict | list:
        """
        Generate JSON response using Gemini.
        
        Args:
            prompt: The user prompt (should ask for JSON output)
            system_prompt: Optional system instruction
            temperature: Override default temperature
            
        Returns:
            Parsed JSON (dict or list)
        """
        try:
            # Add JSON instruction to system prompt
            json_system = f"{system_prompt}\n\nIMPORTANT: Return ONLY valid JSON, no markdown formatting, no explanation." if system_prompt else "Return ONLY valid JSON, no markdown formatting, no explanation."
            
            response_text = self.generate(prompt, json_system, temperature)
            
            # Clean up response - remove markdown code blocks if present
            if "```" in response_text:
                # Extract content between code blocks
                parts = response_text.split("```")
                for part in parts:
                    cleaned = part.replace("json", "").strip()
                    if cleaned.startswith("[") or cleaned.startswith("{"):
                        response_text = cleaned
                        break
            
            return json.loads(response_text)
        except json.JSONDecodeError as e:
            print(f"JSON parsing error: {e}")
            print(f"Raw response: {response_text[:500]}")
            raise
        except Exception as e:
            print(f"LLM JSON generation error: {e}")
            raise


# Lazy singleton getter
_llm_client: Optional[LLMClient] = None


def get_llm_client() -> LLMClient:
    """Get or create the LLM client singleton"""
    global _llm_client
    if _llm_client is None:
        _llm_client = LLMClient()
    return _llm_client


# For backward compatibility - use lazy initialization
class _LLMClientProxy:
    """Proxy that lazily initializes the LLM client"""
    def __getattr__(self, name):
        return getattr(get_llm_client(), name)


llm_client = _LLMClientProxy()
