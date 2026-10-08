using Microsoft.AspNetCore.Http;

namespace POS_Private.Application.DTOs;

/// <summary>
/// Generic base response class for API responses
/// </summary>
/// <typeparam name="T">Type of data in the response</typeparam>
public class APIBaseResponse<T>
{
    public APIBaseResponse()
    {
        StatusCode = StatusCodes.Status200OK;
        ErrorList = new List<string>();
        IsSuccess = true;
    }

    public T? Data { get; set; }
    public string? SuccessMessage { get; set; }
    public int StatusCode { get; set; }
    public List<string> ErrorList { get; set; }
    public bool IsSuccess { get; set; }
    public int? TotalRecords { get; set; }
    public object? ValidationErrorList { get; set; }

    public APIBaseResponse<T> SetError(int statusCode, string message)
    {
        StatusCode = statusCode;
        IsSuccess = false;
        ErrorList.Clear();
        ErrorList.Add(message);
        Data = default;
        return this;
    }

    public APIBaseResponse<T> SetError(int statusCode, List<string> messages)
    {
        StatusCode = statusCode;
        IsSuccess = false;
        ErrorList.Clear();
        ErrorList.AddRange(messages ?? new List<string>());
        Data = default;
        return this;
    }

    public APIBaseResponse<T> SetError(int statusCode, string message, object validationErrors)
    {
        StatusCode = statusCode;
        IsSuccess = false;
        ErrorList.Clear();
        ErrorList.Add(message);
        ValidationErrorList = validationErrors;
        Data = default;
        return this;
    }

    public APIBaseResponse<T> SetSuccess(T data, string? message = null, int? totalRecords = null)
    {
        StatusCode = StatusCodes.Status200OK;
        IsSuccess = true;
        Data = data;
        SuccessMessage = message;
        TotalRecords = totalRecords;
        ErrorList.Clear();
        return this;
    }

    public static APIBaseResponse<T> Success(T data, string? message = null, int? totalRecords = null)
    {
        return new APIBaseResponse<T>().SetSuccess(data, message, totalRecords);
    }

    public static APIBaseResponse<T> Failed(int statusCode, string message)
    {
        return new APIBaseResponse<T>().SetError(statusCode, message);
    }

    public static APIBaseResponse<T> Failed(int statusCode, List<string> messages)
    {
        return new APIBaseResponse<T>().SetError(statusCode, messages);
    }

    public static APIBaseResponse<T> Failure(string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        return new APIBaseResponse<T>().SetError(statusCode, message);
    }

    public static APIBaseResponse<T> Failure(List<string> messages, int statusCode = StatusCodes.Status400BadRequest)
    {
        return new APIBaseResponse<T>().SetError(statusCode, messages);
    }

    public static APIBaseResponse<T> ValidationError(object validationErrors, string message = "Validation failed")
    {
        return new APIBaseResponse<T>().SetError(StatusCodes.Status400BadRequest, message, validationErrors);
    }
}
