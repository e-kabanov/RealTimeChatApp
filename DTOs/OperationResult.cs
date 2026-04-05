namespace RealTimeChatApp.DTOs
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static OperationResult<T> Ok(T data) => new OperationResult<T>()
        { 
            IsSuccess = true,
            Data = data,
            StatusCode = 200
        };
        public static OperationResult<T> Created(T data) => new OperationResult<T>() 
        { 
            IsSuccess = true,
            Data = data,
            StatusCode = 201
        };
        public static OperationResult<T> NoContent() => new OperationResult<T>()
        { 
            IsSuccess = true,
            StatusCode = 204
        };

        public static OperationResult<T> BadRequest(string message) => new()
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = 400
        };

        public static OperationResult<T> Forbidden() => new()
        {
            IsSuccess = false,
            StatusCode = 403
        };

        public static OperationResult<T> NotFound(string message) => new()
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = 404
        };

    }
}
