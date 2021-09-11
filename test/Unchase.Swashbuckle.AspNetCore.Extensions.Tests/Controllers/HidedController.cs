using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Controllers
{
    /// <summary>
    /// Hided controller
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Hided controller")]
    public class HidedController : ControllerBase
    {
        /// <summary>
        /// Hided action
        /// </summary>
        /// <remarks>
        /// Hided action remarks
        /// </remarks>
        [HttpGet("HidedAction")]
        [Authorize(Roles = "NotAcceptedRole")]
        public IActionResult HidedAction()
        {
            return NoContent();
        }
    }
}
