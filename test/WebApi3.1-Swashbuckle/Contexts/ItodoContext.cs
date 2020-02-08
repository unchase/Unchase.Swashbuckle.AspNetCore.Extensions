using System.Collections.Generic;
using TodoApi.Models;

namespace WebApi3._1_Swashbuckle.Contexts
{
    internal interface ItodoContext
    {
        List<TodoItem> TodoItems { get; set; }

        TodoItem Find(long id);

        void Remove(TodoItem item);

        void Add(TodoItem item);
    }
}
