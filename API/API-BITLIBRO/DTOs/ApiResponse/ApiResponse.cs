using System;

namespace API_BITLIBRO.DTOs.ApiResponse;

public class ApiResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Error { get; set; }

    // Métodos estáticos para crear respuestas
    public static ApiResponse Success(string message = "")
    {
        return new ApiResponse { Message = message, Error = false };
    }

    public static ApiResponse Fail(string errorMessage)
    {
        return new ApiResponse { Message = errorMessage, Error = true };
    }
}
