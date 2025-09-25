using System.Text.Json.Serialization;

namespace BancoCentralWeb.Models.Auth
{
    public class PongResponse
    {
        [JsonPropertyName("pong")]
        public decimal Pong { get; set; }
    }
}