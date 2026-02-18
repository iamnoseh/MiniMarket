using System.Net;

namespace Domain.Responses;

public class Responce<T>
{
    public T? Data { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }

    public Responce(T data)
    {
        Data = data;
        Message = "Success";
        StatusCode = (int)HttpStatusCode.OK;
    }

    public Responce(HttpStatusCode statusCode, string message)
    {
        StatusCode = (int)statusCode;
        Message = message;
        Data = default;
    }
}