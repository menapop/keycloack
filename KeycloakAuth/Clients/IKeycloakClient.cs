
using KeycloakAuth.Models;

namespace KeycloakAuth.Clients
{
    public interface IKeycloakClient
    {
        Task<IEnumerable<KeycloakGroup>> GetGroups(CancellationToken cancellationToken);
    }
}
