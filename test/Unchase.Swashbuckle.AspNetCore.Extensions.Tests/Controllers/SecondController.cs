using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models;

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
        /// <param name="tag">Tag parameter.</param>
        /// <param name="empty">Empty string parameter.</param>
        /// <remarks>
        /// Test action remarks
        /// </remarks>
        [HttpGet]
        public IActionResult TestAction(
            Tag tag,
            string empty = "")
        {
            return NoContent();
        }

        /// <inheritdoc cref="TestAction(Tag, string)" path="param|remarks"/>
        /// <summary>
        /// Inherited test action
        /// </summary>
        /// <remarks>
        /// Inherited test action remarks
        /// </remarks>
        [HttpGet("inherited-test")]
        public IActionResult InheritedTestAction(
            Tag tag,
            string empty = "")
        {
            return NoContent();
        }
    }
}
