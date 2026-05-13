namespace ProductBrief.Models;

public class Result<T>
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? HttpCode { get; set; }
    public T? Data { get; set; }

    public static Result<T> Ok(T data)
        => new() { Success = true, Data = data };
    public static Result<T> Fail(string error, string? httpCode)
        => new() { Success = false, Error = error, HttpCode = httpCode};
}
