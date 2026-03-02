using System;

namespace HRHiringSystem.Application.Responses;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T? data, string? message = null)
        => new ApiResponse<T> { IsSuccess = true, Data = data, Message = message };

    public static ApiResponse<T> Failure(string? errorCode = null, string? message = null)
        => new ApiResponse<T> { IsSuccess = false, ErrorCode = errorCode, Message = message };
}
