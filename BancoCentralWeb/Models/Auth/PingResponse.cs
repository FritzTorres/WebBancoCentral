namespace BancoCentralWeb.Models.Auth
{
    public class PingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public PingData? Data { get; set; }
    }

    public class PingData
    {
        public double Pong { get; set; }
    }
}