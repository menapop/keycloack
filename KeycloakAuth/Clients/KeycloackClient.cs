
using KeycloakAuth.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace KeycloakAuth.Clients
{
    public sealed class KeycloakClient (IHttpClientFactory httpClientFactory, ILogger<KeycloakClient> logger) : IKeycloakClient
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("keycloakClient");

        public async Task<IEnumerable<KeycloakGroup>> GetGroups(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync("groups", cancellationToken);

            response.EnsureSuccessStatusCode(); 

            return await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakGroup>>(cancellationToken);

        }
    }
}
