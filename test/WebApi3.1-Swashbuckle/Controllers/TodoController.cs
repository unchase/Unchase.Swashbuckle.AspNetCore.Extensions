using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using TodoApi.Models;
using WebApi3._1_Swashbuckle.Contexts;
using WebApi3._1_Swashbuckle.Models;

namespace WebApi3._1_Swashbuckle.Controllers
{
    /// <summary>
    /// Todo controller
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    //[Authorize(Roles = "NotAcceptedRole")]
    [ApiController]
    [SwaggerTag("Controller for todo")]
    public class TodoController : ControllerBase
    {
        private static readonly ItodoContext _context = new TodoContext();

        /// <summary>
        /// Hided action
        /// </summary>
        /// <remarks>
        /// Hided action remarks
        /// </remarks>
        [HttpGet("hided")]
        [Authorize(Roles = "NotAcceptedRole")]
        public IActionResult HidedAction()
        {
            return NoContent();
        }

        /// <summary>
        /// Complicated action
        /// </summary>
        /// <remarks>
        /// Complicated action remarks
        /// </remarks>
        /// <returns>A complicated class.</returns>
        /// <response code="200">Returns a complicated class.</response>
        [HttpGet("complicated")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ComplicatedClass> ComplicatedAction()
        {
            return Ok(new ComplicatedClass
            {
                Tag = Tag.Task,
                InnerClass = new InnerClass
                {
                    InnerEnum = new List<InnerEnum> { InnerEnum.Value },
                    SecondInnerClass = new SecondInnerClass<SecondInnerEnum>
                    {
                        InnerEnum = SecondInnerEnum.Value
                    }
                }
            });
        }

        /// <summary>
        /// Deletes a specific TodoItem
        /// </summary>
        /// <remarks>
        /// Deletes a specific TodoItem remarks
        /// </remarks>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "AcceptedRole")]
        public IActionResult Delete(long id)
        {
            var todo = _context.Find(id);

            if (todo == null)
            {
                return NotFound();
            }

            _context.Remove(todo);

            return NoContent();
        }

        /// <summary>
        /// Creates a TodoItem
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "id": 1,
        ///        "name": "Item1",
        ///        "isComplete": true,
        ///        "tag": 0
        ///     }
        ///
        /// </remarks>
        /// <param name="item">TodoItem.</param>
        /// <returns>A newly created TodoItem.</returns>
        /// <response code="201">Returns the newly created item.</response>
        /// <response code="400">If the item is null.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TodoItem> Create(TodoItem item)
        {
            _context.Add(item);

            return new CreatedResult(string.Empty, item);
        }
    }
}
