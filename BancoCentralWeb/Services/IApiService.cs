namespace BancoCentralWeb.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, Guid sessionId);
        Task<T?> PostAsync<T>(string endpoint, object data, Guid sessionId);
        Task<bool> DeleteAsync(string endpoint, Guid sessionId);
    }
}