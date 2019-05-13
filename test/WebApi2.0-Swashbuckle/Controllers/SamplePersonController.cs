using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi2._0_Swashbuckle.Models;

namespace WebApi2._0_Swashbuckle.Controllers
{
    /// <summary>
    /// Sample Person Controller.
    /// </summary>
    [SwaggerTag("SamplePerson description")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class SamplePersonController : ControllerBase
    {
        // GET api/values
        /// <summary>
        /// Sample GET-action.
        /// </summary>
        /// <param name="title">Sample Person title.</param>
        /// <returns>Returns sample Person response.</returns>
        /// <response code="200">Returns sample Person response.</response>
        [Authorize(Roles = "AcceptedRole")]
        [HttpGet("get")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SamplePersonRequestResponse), 200)]
        public ActionResult<SamplePersonRequestResponse> Get(Title title)
        {
            var response = new SamplePersonRequestResponse
            {
                Title = title,
                Age = 20,
                FirstName = "Sample firstname",
                Income = 100
            };
            return Ok(response);
        }

        // POST api/values
        /// <summary>
        /// Sample POST-action.
        /// </summary>
        /// <param name="request">Sample Person request.</param>
        /// <returns>Returns sample Person response.</returns>
        /// <response code="200">Returns sample Person response.</response>
        [Authorize(Roles = "NotAcceptedRole")]
        [HttpPost("post")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SamplePersonRequestResponse), 200)]
        public ActionResult<SamplePersonRequestResponse> Post([FromBody] SamplePersonRequestResponse request)
        {
            return Ok(request);
        }
    }
}
