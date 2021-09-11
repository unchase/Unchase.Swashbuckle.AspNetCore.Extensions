using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Controllers
{
    /// <summary>
    /// Second controller
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Second controller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(420)]
    public class SecondController : ControllerBase
    {
        /// <summary>
        /// Test action
        /// </summary>
        /// <remarks>
        /// Test action remarks
        /// </remarks>
        [HttpGet()]
        public IActionResult TestAction()
        {
            return NoContent();
        }
    }
}
