import httpx
from urllib.parse import urlparse
from azure.storage.blob import BlobServiceClient

from app.core.config import get_settings


class BlobService:
    """Service for downloading files from Azure Blob Storage"""
    
    def __init__(self):
        settings = get_settings()
        if settings.azure_blob_connection_string:
            self.client = BlobServiceClient.from_connection_string(
                settings.azure_blob_connection_string
            )
        else:
            self.client = None
    
    async def download_cv(self, blob_url: str) -> tuple[bytes, str]:
        """
        Download CV file from Azure Blob Storage URL.
        Returns tuple of (content_bytes, filename)
        """
        # Parse the blob URL to extract container and blob name
        parsed = urlparse(blob_url)
        
        # Handle different URL formats
        if self.client and "blob.core.windows.net" in blob_url:
            # Azure Blob URL format: https://account.blob.core.windows.net/container/blob
            path_parts = parsed.path.lstrip('/').split('/', 1)
            if len(path_parts) == 2:
                container_name, blob_name = path_parts
                return await self._download_from_azure(container_name, blob_name)
        
        # Fallback: Direct HTTP download
        return await self._download_http(blob_url)
    
    async def _download_from_azure(self, container_name: str, blob_name: str) -> tuple[bytes, str]:
        """Download from Azure Blob Storage using SDK"""
        try:
            blob_client = self.client.get_blob_client(
                container=container_name, 
                blob=blob_name
            )
            data = blob_client.download_blob().readall()
            filename = blob_name.split('/')[-1]
            return data, filename
        except Exception as e:
            raise RuntimeError(f"Failed to download from Azure Blob: {e}")
    
    async def _download_http(self, url: str) -> tuple[bytes, str]:
        """Download file via HTTP"""
        try:
            # Replace localhost with Docker network hostname
            settings = get_settings()
            if "localhost" in url:
                url = url.replace("http://localhost:8080", settings.dotnet_api_url)
                url = url.replace("https://localhost:8080", settings.dotnet_api_url)
            
            async with httpx.AsyncClient() as client:
                response = await client.get(url, timeout=60.0)
                response.raise_for_status()
                
                # Extract filename from URL or content-disposition
                filename = url.split('/')[-1].split('?')[0]
                if not filename:
                    filename = "cv.pdf"
                
                return response.content, filename
        except Exception as e:
            raise RuntimeError(f"Failed to download CV via HTTP: {e}")


# Singleton instance
blob_service = BlobService()
