using KeycloakAuth.Exceptions;
using KeycloakAuth.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KeycloakAuth.Handlers
{
    public class AttachAccessTokenHandler(IHttpClientFactory _httpClientFactory,IOptions<KeycloakOptions> _KeycloakOptions,IMemoryCache _memoryCache) : DelegatingHandler
    {
        private readonly string _cachekey = $"access-token-{_KeycloakOptions.Value.Credentials.ClientId}";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_memoryCache.TryGetValue(_cachekey, out Token? token))
            {
                HttpResponseMessage tokenResponse = await _httpClientFactory.CreateClient("keycloakAccessTokenClient").PostAsync($"realms/{_KeycloakOptions.Value.Realm}/protocol/openid-connect/token", new FormUrlEncodedContent ( new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id",_KeycloakOptions.Value.Credentials.ClientId ),
                        new KeyValuePair<string, string>("client_secret", _KeycloakOptions.Value.Credentials.ClientSecret)
                    })
                , cancellationToken);

                tokenResponse.EnsureSuccessStatusCode();    

                token  = await tokenResponse.Content.ReadFromJsonAsync<Token>(cancellationToken);  

                if (token is null )
                {
                    throw new AccessTokenGenerateException();
                }
                _memoryCache.Set(_cachekey, token,new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(token.expires_in-30))); 

            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token!.access_token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
