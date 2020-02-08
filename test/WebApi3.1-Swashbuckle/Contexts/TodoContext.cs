using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;

namespace WebApi3._1_Swashbuckle.Contexts
{
    /// <summary>
    /// Context for todo items.
    /// </summary>
    internal class TodoContext : ItodoContext
    {
        private List<TodoItem> _todoItems;

        /// <summary>
        /// List of todo items.
        /// </summary>
        public List<TodoItem> TodoItems
        {
            get
            {
                if (_todoItems == null)
                    _todoItems = new List<TodoItem>();
                return _todoItems;
            }
            set
            {
                _todoItems = value.ToList();
            }
        }

        /// <summary>
        /// Add todo item.
        /// </summary>
        /// <param name="item"><see cref="TodoItem"/>.</param>
        public void Add(TodoItem item)
        {
            TodoItems.Add(item);
        }

        /// <summary>
        /// Find todo item by id.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>
        /// Returns founded todo item.
        /// </returns>
        public TodoItem Find(long id)
        {
            return TodoItems.FirstOrDefault(i => i.Id == id);
        }

        /// <summary>
        /// Remove todo item.
        /// </summary>
        /// <param name="item"><see cref="TodoItem"/>.</param>
        public void Remove(TodoItem item)
        {
            TodoItems.Remove(item);
        }
    }
}
