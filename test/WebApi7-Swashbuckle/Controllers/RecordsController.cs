using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi7_Swashbuckle.Models;

namespace WebApi7_Swashbuckle.Controllers
{
    /// <summary>
    /// Records controller
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Records controller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(420)]
    public class RecordsController : ControllerBase
    {
        /// <summary>
        /// Get record response action
        /// </summary>
        /// <remarks>
        /// Get record response action remarks
        /// </remarks>
        [HttpGet("get-record-response")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<RecordResponse> GetRecordResponseAction()
        {
            return Ok(new RecordResponse(5, "name1"));
        }
    }
}
