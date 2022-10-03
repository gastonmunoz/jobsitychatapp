using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Mvc;

namespace JobsityChatAngular.Controllers
{
    /// <summary>
    /// Identity controller
    /// </summary>
    public class OidcConfigurationController : Controller
    {
        private readonly ILogger<OidcConfigurationController> _logger;

        public OidcConfigurationController(
            IClientRequestParametersProvider clientRequestParametersProvider,
            ILogger<OidcConfigurationController> logger)
        {
            ClientRequestParametersProvider = clientRequestParametersProvider;
            _logger = logger;
        }

        /// <summary>
        /// Parameters for the authentication request
        /// </summary>
        public IClientRequestParametersProvider ClientRequestParametersProvider { get; }

        /// <summary>
        /// Returns parameters for the authentication request
        /// </summary>
        /// <param name="clientId">Client identification</param>
        /// <returns></returns>
        [HttpGet("_configuration/{clientId}")]
        public IActionResult GetClientRequestParameters([FromRoute] string clientId)
        {
            IDictionary<string, string> parameters = ClientRequestParametersProvider.GetClientParameters(HttpContext, clientId);
            return Ok(parameters);
        }
    }
}