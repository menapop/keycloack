using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace KeycloakAuth.ClaimsTransformations
{
    public class KeycloakRolesClaimsTransformation(string roleClaimType, string audience) : IClaimsTransformation
    {
        private readonly string _roleClaimType = roleClaimType;
        private readonly string _audience = audience;

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var result = principal.Clone();
            if (result.Identity is not ClaimsIdentity identity)
            {
                return Task.FromResult(result);
            }

            var resourceAccessValue = principal.FindFirst("resource_access")?.Value;
            if (string.IsNullOrWhiteSpace(resourceAccessValue))
            {
                return Task.FromResult(result);
            }

            using var resourceAccess = JsonDocument.Parse(resourceAccessValue);
            var clientRoles = resourceAccess
                .RootElement
                .GetProperty(_audience)
                .GetProperty(_roleClaimType);

            foreach (var role in clientRoles.EnumerateArray())
            {
                var value = role.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    identity.AddClaim(new Claim(_roleClaimType, value));
                }
            }

            return Task.FromResult(result);
        }
    }
}
