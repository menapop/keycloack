using KeycloakAuth.Clients;
using KeycloakAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace keycloack.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController(IKeycloakClient keycloakService) : ControllerBase
    {
        
        private readonly IKeycloakClient _keycloakService = keycloakService;

        [HttpGet]
        [Authorize(Roles ="Students")]
        public async Task<IEnumerable<KeycloakGroup>> GetAllGroups(CancellationToken cancellationToken)
        {
            var x = User; 

            return await _keycloakService.GetGroups(cancellationToken);
        }


      
      
    }
}
