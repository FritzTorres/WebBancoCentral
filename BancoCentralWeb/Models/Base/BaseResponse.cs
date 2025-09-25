namespace BancoCentralWeb.Models.Base
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public object? Data { get; set; }
    }
}