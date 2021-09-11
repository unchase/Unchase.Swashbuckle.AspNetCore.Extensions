using System.Collections.Generic;
using System.Linq;
using Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Contexts
{
    /// <inheritdoc cref="ItodoContext"/>
    public class TodoContext : ItodoContext
    {
        private List<TodoItem> _todoItems;

        /// <inheritdoc/>
        public List<TodoItem> TodoItems
        {
            get => _todoItems ?? (_todoItems = new List<TodoItem>());
            set => _todoItems = value.ToList();
        }

        /// <inheritdoc/>
        public void Add(TodoItem item)
        {
            TodoItems.Add(item);
        }

        /// <inheritdoc/>
        public TodoItem Find(long id)
        {
            return TodoItems.FirstOrDefault(i => i.Id == id);
        }

        /// <inheritdoc/>
        public void Remove(TodoItem item)
        {
            TodoItems.Remove(item);
        }
    }
}
