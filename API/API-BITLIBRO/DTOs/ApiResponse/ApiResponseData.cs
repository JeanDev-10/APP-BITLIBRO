using System;

namespace API_BITLIBRO.DTOs.ApiResponse;

public class ApiResponseData<T> : ApiResponse
{
    public T? Data { get; set; }

    // Métodos estáticos para crear respuestas con datos
    public static ApiResponseData<T> Success(T data, string message = "")
    {
        return new ApiResponseData<T>
        {
            Message = message,
            Error = false,
            Data = data
        };
    }

    public static ApiResponseData<T> Fail(string errorMessage, T? data = default)
    {
        return new ApiResponseData<T>
        {
            Message = errorMessage,
            Error = true,
            Data = data
        };
    }
}
