using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BancoCentralWeb.Models.Auth;
using BancoCentralWeb.Models.Clientes;

namespace BancoCentralWeb.Services
{
    public class ApiService : IApiService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiService> _logger;
        private readonly string _baseUrl;

        public ApiService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("API BaseUrl not configured");
        }

        public async Task<T?> GetAsync<T>(string endpoint, Guid sessionId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Session-Id", sessionId.ToString());
            
            var url = $"{_baseUrl}{endpoint}";
            _logger.LogInformation("GET Request a: {Url}", url);
            
            var response = await client.GetAsync(url);
            
            _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response Content: {Content}", content);
                return ParseApiResponse<T>(content);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error Response: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }
            
            return default(T)!;
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, Guid sessionId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Session-Id", sessionId.ToString());
            
            var url = $"{_baseUrl}{endpoint}";
            _logger.LogInformation("POST Request a: {Url}", url);
            
            var json = JsonSerializer.Serialize(data);
            _logger.LogInformation("Request Data: {Json}", json);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(url, content);
            
            _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response Content: {Content}", responseContent);
                return ParseApiResponse<T>(responseContent);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error Response: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }
            
            return default(T)!;
        }

        public async Task<bool> DeleteAsync(string endpoint, Guid sessionId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Session-Id", sessionId.ToString());
            
            var response = await client.DeleteAsync($"{_baseUrl}{endpoint}");
            return response.IsSuccessStatusCode;
        }

        private T? ParseApiResponse<T>(string content)
        {
            _logger.LogInformation("ParseApiResponse recibiendo contenido: {Content}", content);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Contenido vacío en ParseApiResponse");
                return default(T)!;
            }

            try
            {
                // Intentar parsear como JSON directamente (formato nuevo de la API)
                using (JsonDocument document = JsonDocument.Parse(content))
                {
                    var root = document.RootElement;
                    
                    // Verificar si tiene la estructura {success: true, data: {...}}
                    if (root.TryGetProperty("success", out var successProp) && 
                        successProp.GetBoolean() &&
                        root.TryGetProperty("data", out var dataProp))
                    {
                        _logger.LogInformation("Detectado formato JSON con estructura success/data");
                        
                        // Extraer los datos del objeto data
                        var dataElement = dataProp.GetRawText();
                        _logger.LogInformation("Data element: {DataElement}", dataElement);
                        
                        // Manejo especial para ClienteListResponse - formato plano con prefijos
                        if (typeof(T) == typeof(ClienteListResponse))
                        {
                            _logger.LogInformation("Detectado tipo ClienteListResponse, procesando formato plano con prefijos");
                            return ParseClienteListResponse<T>(dataElement);
                        }
                        
                        // Mapear los nombres de propiedades si es necesario
                        var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(dataElement);
                        if (dataDict != null)
                        {
                            // Mapear session_id a SessionId y expira_en a ExpiraEn
                            if (dataDict.ContainsKey("session_id"))
                            {
                                dataDict["SessionId"] = dataDict["session_id"];
                                dataDict.Remove("session_id");
                            }
                            if (dataDict.ContainsKey("expira_en"))
                            {
                                dataDict["ExpiraEn"] = dataDict["expira_en"];
                                dataDict.Remove("expira_en");
                            }
                            
                            var mappedJson = JsonSerializer.Serialize(dataDict);
                            _logger.LogInformation("JSON mapeado: {MappedJson}", mappedJson);
                            
                            var result = JsonSerializer.Deserialize<T>(mappedJson);
                            _logger.LogInformation("Deserialización exitosa desde formato JSON");
                            
                            // Log adicional para depuración de PongResponse
                            if (typeof(T) == typeof(PongResponse))
                            {
                                _logger.LogInformation("Tipo detectado: PongResponse, resultado: {Result}", result != null ? "no null" : "null");
                                if (result != null)
                                {
                                    var jsonResult = JsonSerializer.Serialize(result);
                                    _logger.LogInformation("PongResponse serializado: {JsonResult}", jsonResult);
                                }
                            }
                            
                            return result;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No se detectó formato JSON success/data, intentando formato legacy");
                        
                        // Intentar con el formato legacy: OK|clave=valor|clave2=valor2
                        var parts = content.Split('|');
                        if (parts.Length > 0)
                        {
                            var status = parts[0];
                            _logger.LogInformation("Status recibido (legacy): {Status}", status);
                            
                            if (status == "OK")
                            {
                                var data = new Dictionary<string, object>();
                                
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    var keyValue = parts[i].Split('=');
                                    if (keyValue.Length == 2)
                                    {
                                        var key = keyValue[0];
                                        var value = keyValue[1];
                                        _logger.LogInformation("Procesando par (legacy): {Key}={Value}", key, value);

                                        // Mapear nombres de propiedades de la API a los del modelo
                                        var mappedKey = key switch
                                        {
                                            "session_id" => "SessionId",
                                            "expira_en" => "ExpiraEn",
                                            _ => key
                                        };

                                        _logger.LogInformation("Key mapeado (legacy): {MappedKey}", mappedKey);

                                        // Convertir tipos comunes
                                        if (Guid.TryParse(value, out Guid guidValue))
                                        {
                                            data[mappedKey] = guidValue;
                                        }
                                        else if (DateTime.TryParse(value, out DateTime dateValue))
                                        {
                                            data[mappedKey] = dateValue;
                                        }
                                        else if (decimal.TryParse(value, out decimal decimalValue))
                                        {
                                            data[mappedKey] = decimalValue;
                                        }
                                        else if (int.TryParse(value, out int intValue))
                                        {
                                            data[mappedKey] = intValue;
                                        }
                                        else
                                        {
                                            data[mappedKey] = value;
                                        }
                                    }
                                }

                                // Convertir el diccionario al tipo T usando System.Text.Json
                                var json = JsonSerializer.Serialize(data);
                                _logger.LogInformation("JSON generado (legacy): {Json}", json);
                                
                                var result = JsonSerializer.Deserialize<T>(json);
                                _logger.LogInformation("Deserialización exitosa (legacy)");
                                return result;
                            }
                            else
                            {
                                _logger.LogWarning("Status no es OK (legacy): {Status}", status);
                            }
                        }
                    }
                }
                
                // Si llegamos aquí, intentar deserializar directamente el contenido completo
                _logger.LogInformation("Intentando deserializar directamente el contenido completo");
                var directResult = JsonSerializer.Deserialize<T>(content);
                if (directResult != null)
                {
                    _logger.LogInformation("Deserialización directa exitosa");
                    return directResult;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear JSON en ParseApiResponse");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en ParseApiResponse");
            }
            
            _logger.LogWarning("No se pudo parsear la respuesta");
            return default(T)!;
        }

        private T ParseClienteListResponse<T>(string dataElement)
        {
            _logger.LogInformation("ParseClienteListResponse - Procesando datos de clientes");
            
            try
            {
                // Parsear el data element como un diccionario para procesar los prefijos
                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(dataElement);
                if (dataDict == null)
                {
                    _logger.LogWarning("No se pudo deserializar el data element como diccionario");
                    return default(T)!;
                }

                var clientes = new List<ClienteResponse>();
                var maxIndex = 0;

                // Encontrar el máximo índice de cliente (r1_, r2_, etc.)
                foreach (var key in dataDict.Keys)
                {
                    if (key.StartsWith("r") && key.Contains("_"))
                    {
                        var parts = key.Split('_');
                        if (parts.Length >= 2 && int.TryParse(parts[0].Substring(1), out int index))
                        {
                            maxIndex = Math.Max(maxIndex, index);
                        }
                    }
                }

                _logger.LogInformation("Detectados {MaxIndex} clientes", maxIndex);

                // Procesar cada cliente
                for (int i = 1; i <= maxIndex; i++)
                {
                    var prefix = $"r{i}_";
                    var clienteDict = new Dictionary<string, object>();

                    foreach (var kvp in dataDict)
                    {
                        if (kvp.Key.StartsWith(prefix))
                        {
                            var propertyName = kvp.Key.Substring(prefix.Length);
                            clienteDict[propertyName] = kvp.Value;
                        }
                    }

                    if (clienteDict.Count > 0)
                    {
                        // Procesar y convertir los valores del cliente
                        var processedClienteDict = ProcessClienteDictionary(clienteDict);
                        
                        // Mapear propiedades del cliente
                        var clienteJson = JsonSerializer.Serialize(processedClienteDict);
                        _logger.LogInformation("Cliente {Index} JSON procesado: {Json}", i, clienteJson);
                        
                        var cliente = JsonSerializer.Deserialize<ClienteResponse>(clienteJson);
                        if (cliente != null)
                        {
                            // Post-procesamiento: manejar DateTime.MinValue como nulo para KycVigenteHasta
                            if (cliente.KycVigenteHasta == DateTime.MinValue)
                            {
                                cliente.KycVigenteHasta = null;
                            }
                            
                            clientes.Add(cliente);
                            _logger.LogInformation("Cliente {Index} procesado: {Nombre}", i, cliente.NombreCompleto);
                        }
                    }
                }

                // Crear el response final
                var response = new ClienteListResponse
                {
                    Clientes = clientes,
                    Page = GetIntFromValue(dataDict, "page", 1),
                    PageSize = GetIntFromValue(dataDict, "page_size", 10),
                    Total = GetIntFromValue(dataDict, "total", clientes.Count)
                };

                _logger.LogInformation("ClienteListResponse creado con {Count} clientes", response.Clientes.Count);

                // Convertir al tipo T esperado
                var responseJson = JsonSerializer.Serialize(response);
                var result = JsonSerializer.Deserialize<T>(responseJson);
                return result ?? default(T)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ParseClienteListResponse");
                return default(T)!;
            }
        }

        private int GetIntFromValue(Dictionary<string, object> dict, string key, int defaultValue)
        {
            if (!dict.ContainsKey(key))
                return defaultValue;

            var value = dict[key];
            if (value == null)
                return defaultValue;

            try
            {
                // Si es un JsonElement, obtener el valor como int
                if (value is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                        return jsonElement.GetInt32();
                    else if (jsonElement.ValueKind == JsonValueKind.String)
                        return int.Parse(jsonElement.GetString() ?? defaultValue.ToString());
                }
                // Si ya es un número, convertirlo directamente
                else if (value is IConvertible convertible)
                {
                    return convertible.ToInt32(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al convertir valor '{Key}' a int: {Value}", key, value);
            }

            return defaultValue;
        }

        private Dictionary<string, object> ProcessClienteDictionary(Dictionary<string, object> clienteDict)
        {
            var processedDict = new Dictionary<string, object>();
            
            foreach (var kvp in clienteDict)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                string mappedKey = string.Empty; // Declarar fuera del try para tener alcance correcto
                
                try
                {
                    // Mapear nombres de propiedades
                    mappedKey = key switch
                    {
                        "cliente_id" => "ClienteId",
                        "cedula_rnc" => "CedulaRnc",
                        "nombre_completo" => "NombreCompleto",
                        "tipo_cliente" => "TipoCliente",
                        "kyc_vigente_hasta" => "KycVigenteHasta",
                        "creado_en" => "CreadoEn",
                        _ => key
                    };
                    
                    // Procesar el valor según su tipo
                    if (value is JsonElement jsonElement)
                    {
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.String:
                                var stringValue = jsonElement.GetString() ?? string.Empty;
                                
                                // Manejar GUIDs
                                if (mappedKey == "ClienteId")
                                {
                                    if (Guid.TryParse(stringValue, out Guid guidValue))
                                    {
                                        processedDict[mappedKey] = guidValue;
                                    }
                                    else
                                    {
                                        processedDict[mappedKey] = Guid.NewGuid(); // Generar uno nuevo si no es válido
                                    }
                                }
                                // Manejar fechas
                                else if (mappedKey == "CreadoEn")
                                {
                                    if (DateTime.TryParse(stringValue, out DateTime dateValue))
                                    {
                                        processedDict[mappedKey] = dateValue;
                                    }
                                    else
                                    {
                                        processedDict[mappedKey] = DateTime.UtcNow; // Valor por defecto
                                    }
                                }
                                else if (mappedKey == "KycVigenteHasta")
                                {
                                    if (DateTime.TryParse(stringValue, out DateTime dateValue))
                                    {
                                        processedDict[mappedKey] = dateValue;
                                    }
                                    else
                                    {
                                        // Para valores nulos, usamos una fecha mínima como indicador
                                        processedDict[mappedKey] = DateTime.MinValue;
                                    }
                                }
                                else
                                {
                                    processedDict[mappedKey] = stringValue;
                                }
                                break;
                                
                            case JsonValueKind.Number:
                                // Para cédulas que pueden ser números o strings
                                if (mappedKey == "CedulaRnc")
                                {
                                    processedDict[mappedKey] = jsonElement.GetRawText();
                                }
                                else
                                {
                                    processedDict[mappedKey] = jsonElement.GetDouble();
                                }
                                break;
                                
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                processedDict[mappedKey] = jsonElement.GetBoolean();
                                break;
                                
                            case JsonValueKind.Null:
                                if (mappedKey == "KycVigenteHasta")
                                {
                                    processedDict[mappedKey] = DateTime.MinValue; // Indicador de nulo
                                }
                                else
                                {
                                    processedDict[mappedKey] = string.Empty;
                                }
                                break;
                                
                            default:
                                processedDict[mappedKey] = jsonElement.GetRawText();
                                break;
                        }
                    }
                    else if (value is string strValue)
                    {
                        // Manejar GUIDs
                        if (mappedKey == "ClienteId")
                        {
                            if (Guid.TryParse(strValue, out Guid guidValue))
                            {
                                processedDict[mappedKey] = guidValue;
                            }
                            else
                            {
                                processedDict[mappedKey] = Guid.NewGuid(); // Generar uno nuevo si no es válido
                            }
                        }
                        // Manejar fechas
                        else if (mappedKey == "CreadoEn")
                        {
                            if (DateTime.TryParse(strValue, out DateTime dateValue))
                            {
                                processedDict[mappedKey] = dateValue;
                            }
                            else
                            {
                                processedDict[mappedKey] = DateTime.UtcNow; // Valor por defecto
                            }
                        }
                        else if (mappedKey == "KycVigenteHasta")
                        {
                            if (DateTime.TryParse(strValue, out DateTime dateValue))
                            {
                                processedDict[mappedKey] = dateValue;
                            }
                            else
                            {
                                // Para valores nulos, usamos una fecha mínima como indicador
                                processedDict[mappedKey] = DateTime.MinValue;
                            }
                        }
                        else
                        {
                            processedDict[mappedKey] = strValue;
                        }
                    }
                    else if (value is DateTime dateTimeValue)
                    {
                        processedDict[mappedKey] = dateTimeValue;
                    }
                    else if (value is IConvertible convertible)
                    {
                        // Para GUIDs
                        if (mappedKey == "ClienteId")
                        {
                            try
                            {
                                var stringValue = convertible.ToString();
                                if (Guid.TryParse(stringValue, out Guid guidValue))
                                {
                                    processedDict[mappedKey] = guidValue;
                                }
                                else
                                {
                                    processedDict[mappedKey] = Guid.NewGuid();
                                }
                            }
                            catch
                            {
                                processedDict[mappedKey] = Guid.NewGuid();
                            }
                        }
                        // Para cédulas que pueden ser números o strings
                        else if (mappedKey == "CedulaRnc")
                        {
                            processedDict[mappedKey] = convertible.ToString() ?? string.Empty;
                        }
                        else if (mappedKey == "CreadoEn" || mappedKey == "KycVigenteHasta")
                        {
                            try
                            {
                                if (mappedKey == "KycVigenteHasta" && convertible.ToString() == "")
                                {
                                    processedDict[mappedKey] = DateTime.MinValue; // Indicador de nulo
                                }
                                else
                                {
                                    processedDict[mappedKey] = convertible.ToDateTime(System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }
                            catch
                            {
                                processedDict[mappedKey] = mappedKey == "KycVigenteHasta" ? DateTime.MinValue : DateTime.UtcNow;
                            }
                        }
                        else
                        {
                            processedDict[mappedKey] = convertible;
                        }
                    }
                    else
                    {
                        processedDict[mappedKey] = value ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al procesar propiedad '{Key}' con valor '{Value}'", key, value);
                    // Usar valor por defecto o vacío
                    processedDict[mappedKey] = string.Empty;
                }
            }
            
            return processedDict;
        }
    }
}