using System.Net.Http.Json;
using System.Text.Json;
using KnjiznicaBlazor.Models;
using Microsoft.Extensions.Logging;

namespace KnjiznicaBlazor.Services
{
    public class KnjiznicaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KnjiznicaService>? _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public KnjiznicaService(HttpClient httpClient, ILogger<KnjiznicaService>? logger = null)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Helper method for handling HTTP errors
        private async Task<T> HandleResponse<T>(HttpResponseMessage response, string operation)
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(content))
                    {
                        return default(T)!;
                    }

                    return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = $"API call failed for {operation}. Status: {response.StatusCode}";

                if (!string.IsNullOrEmpty(errorContent))
                {
                    errorMessage += $". Error: {errorContent}";
                }

                _logger?.LogError(errorMessage);
                throw new HttpRequestException(errorMessage);
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "JSON deserialization failed for {Operation}", operation);
                throw new InvalidOperationException($"Invalid JSON response for {operation}", ex);
            }
        }

        // Helper method for error handling
        private async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> operation, string operationName)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "HTTP error in {OperationName}", operationName);
                throw new HttpRequestException($"Napaka pri povezavi z API ({operationName}): {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger?.LogError(ex, "Timeout in {OperationName}", operationName);
                throw new TimeoutException($"Časovna omejitev prekoračena ({operationName})", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error in {OperationName}", operationName);
                throw;
            }
        }

        // Avtorji
        public async Task<List<Avtor>> GetAvtorjiAsync(string? ime = null, string? priimek = null, string? email = null)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(ime)) queryParams.Add($"ime={Uri.EscapeDataString(ime)}");
                if (!string.IsNullOrEmpty(priimek)) queryParams.Add($"priimek={Uri.EscapeDataString(priimek)}");
                if (!string.IsNullOrEmpty(email)) queryParams.Add($"email={Uri.EscapeDataString(email)}");

                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/avtorji{query}");

                return await HandleResponse<List<Avtor>>(response, "GetAvtorji") ?? new List<Avtor>();
            }, "GetAvtorji");
        }

        public async Task<Avtor?> GetAvtorAsync(int id)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.GetAsync($"api/avtorji/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                return await HandleResponse<Avtor>(response, "GetAvtor");
            }, "GetAvtor");
        }

        public async Task<Avtor> CreateAvtorAsync(Avtor avtor)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync("api/avtorji", avtor, _jsonOptions);
                return await HandleResponse<Avtor>(response, "CreateAvtor");
            }, "CreateAvtor");
        }

        public async Task UpdateAvtorAsync(int id, Avtor avtor)
        {
            await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.PutAsJsonAsync($"api/avtorji/{id}", avtor, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponse<object>(response, "UpdateAvtor");
                }

                return Task.CompletedTask;
            }, "UpdateAvtor");
        }

        public async Task DeleteAvtorAsync(int id)
        {
            await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.DeleteAsync($"api/avtorji/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponse<object>(response, "DeleteAvtor");
                }

                return Task.CompletedTask;
            }, "DeleteAvtor");
        }

        // Kategorije
        public async Task<List<Kategorija>> GetKategorijeAsync(string? ime = null)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var query = !string.IsNullOrEmpty(ime) ? $"?ime={Uri.EscapeDataString(ime)}" : "";
                var response = await _httpClient.GetAsync($"api/kategorije{query}");

                return await HandleResponse<List<Kategorija>>(response, "GetKategorije") ?? new List<Kategorija>();
            }, "GetKategorije");
        }

        public async Task<Kategorija?> GetKategorijaAsync(int id)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.GetAsync($"api/kategorije/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                return await HandleResponse<Kategorija>(response, "GetKategorija");
            }, "GetKategorija");
        }

        public async Task<Kategorija> CreateKategorijaAsync(Kategorija kategorija)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync("api/kategorije", kategorija, _jsonOptions);
                return await HandleResponse<Kategorija>(response, "CreateKategorija");
            }, "CreateKategorija");
        }

        public async Task UpdateKategorijaAsync(int id, Kategorija kategorija)
        {
            await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.PutAsJsonAsync($"api/kategorije/{id}", kategorija, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponse<object>(response, "UpdateKategorija");
                }

                return Task.CompletedTask;
            }, "UpdateKategorija");
        }

        public async Task DeleteKategorijaAsync(int id)
        {
            await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.DeleteAsync($"api/kategorije/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponse<object>(response, "DeleteKategorija");
                }

                return Task.CompletedTask;
            }, "DeleteKategorija");
        }

        // Knjige
        public async Task<List<Knjiga>> GetKnjigeAsync(string? naslov = null, string? avtor = null, string? kategorija = null, string? isbn = null)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(naslov)) queryParams.Add($"naslov={Uri.EscapeDataString(naslov)}");
                if (!string.IsNullOrEmpty(avtor)) queryParams.Add($"avtor={Uri.EscapeDataString(avtor)}");
                if (!string.IsNullOrEmpty(kategorija)) queryParams.Add($"kategorija={Uri.EscapeDataString(kategorija)}");
                if (!string.IsNullOrEmpty(isbn)) queryParams.Add($"isbn={Uri.EscapeDataString(isbn)}");

                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/knjige{query}");

                return await HandleResponse<List<Knjiga>>(response, "GetKnjige") ?? new List<Knjiga>();
            }, "GetKnjige");
        }

        public async Task<Knjiga?> GetKnjigaAsync(int id)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.GetAsync($"api/knjige/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                return await HandleResponse<Knjiga>(response, "GetKnjiga");
            }, "GetKnjiga");
        }

        public async Task<Knjiga> CreateKnjigaAsync(Knjiga knjiga)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync("api/knjige", knjiga, _jsonOptions);
                return await HandleResponse<Knjiga>(response, "CreateKnjiga");
            }, "CreateKnjiga");
        }

        public async Task UpdateKnjigaAsync(int id, Knjiga knjiga)
        {
            await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.PutAsJsonAsync($"api/knjige/{id}", knjiga, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponse<object>(response, "UpdateKnjiga");
                }

                return Task.CompletedTask;
            }, "UpdateKnjiga");
        }

        public async Task DeleteKnjigaAsync(int id)
        {
            await ExecuteWithErrorHandling(async () =>
            {
                var response = await _httpClient.DeleteAsync($"api/knjige/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponse<object>(response, "DeleteKnjiga");
                }

                return Task.CompletedTask;
            }, "DeleteKnjiga");
        }

        // Health check method
        public async Task<bool> IsApiHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/avtorji", HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}