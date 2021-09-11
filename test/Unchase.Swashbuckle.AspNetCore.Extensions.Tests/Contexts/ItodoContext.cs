using System.Collections.Generic;
using Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Contexts
{
    /// <summary>
    /// Context for todo items.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface ItodoContext
    {
        /// <summary>
        /// List of todo items.
        /// </summary>
        List<TodoItem> TodoItems { get; set; }

        /// <summary>
        /// Find todo item by id.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>
        /// Returns founded todo item.
        /// </returns>
        TodoItem Find(long id);

        /// <summary>
        /// Remove todo item.
        /// </summary>
        /// <param name="item"><see cref="TodoItem"/>.</param>
        void Remove(TodoItem item);

        /// <summary>
        /// Add todo item.
        /// </summary>
        /// <param name="item"><see cref="TodoItem"/>.</param>
        void Add(TodoItem item);
    }
}
