using BancoCentralWeb.Models.Auth;
using BancoCentralWeb.Models.Base;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BancoCentralWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IApiService apiService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _apiService = apiService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LoginResponse?> LoginAsync(string usuario, string contrasena)
        {
            var request = new LoginRequest
            {
                Usuario = usuario,
                Contrasena = contrasena
            };

            var result = await _apiService.PostAsync<LoginResponse>("auth/login", request, Guid.Empty);
            
            // Log para depuración
            if (result != null)
            {
                Console.WriteLine($"AuthService.LoginAsync - LoginResponse recibido: SessionId={result.SessionId}, ExpiraEn={result.ExpiraEn}");
            }
            else
            {
                Console.WriteLine("AuthService.LoginAsync - LoginResponse es null");
            }
            
            return result;
        }

        public async Task<bool> LogoutAsync(Guid sessionId)
        {
            var request = new LogoutRequest { SessionId = sessionId };
            var result = await _apiService.PostAsync<BaseResponse>("auth/logout", request, sessionId);
            return result?.Success ?? false;
        }

        public async Task<bool> IsSessionValidAsync(Guid sessionId)
        {
            try
            {
                // Vamos a obtener la respuesta como texto y parsearla manualmente
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId.ToString());
                
                var url = $"{_configuration["ApiSettings:BaseUrl"]}auth/ping";
                var response = await client.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"AuthService.IsSessionValidAsync - Contenido crudo: {content}");
                    
                    // Parsear manualmente el JSON para extraer el valor de pong
                    using (JsonDocument document = JsonDocument.Parse(content))
                    {
                        var root = document.RootElement;
                        
                        if (root.TryGetProperty("success", out var successProp) && 
                            successProp.GetBoolean() &&
                            root.TryGetProperty("data", out var dataProp) &&
                            dataProp.TryGetProperty("pong", out var pongProp))
                        {
                            var pongValue = pongProp.GetDecimal();
                            Console.WriteLine($"AuthService.IsSessionValidAsync - Pong extraído manualmente: {pongValue}");
                            return pongValue == 1.0m;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AuthService.IsSessionValidAsync - Excepción: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(Guid sessionId, string permisoClave)
        {
            try
            {
                var result = await _apiService.GetAsync<BaseResponse>($"auth/permission/{permisoClave}", sessionId);
                return result?.Success ?? false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Guid> GetUserIdAsync(Guid sessionId)
        {
            try
            {
                var result = await _apiService.GetAsync<BaseResponse>("auth/user", sessionId);
                if (result?.Success == true && result.Data != null)
                {
                    // Parsear el data para obtener el user_id
                    var dataJson = System.Text.Json.JsonSerializer.Serialize(result.Data);
                    var dataDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(dataJson);
                    if (dataDict != null && dataDict.ContainsKey("user_id"))
                    {
                        if (Guid.TryParse(dataDict["user_id"].ToString(), out Guid userId))
                        {
                            return userId;
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores
            }
            return Guid.Empty;
        }
    }
}