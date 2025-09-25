namespace BancoCentralWeb.Models.Auth
{
    public class LoginResponse
    {
        public Guid SessionId { get; set; }
        public DateTime? ExpiraEn { get; set; }
    }
}