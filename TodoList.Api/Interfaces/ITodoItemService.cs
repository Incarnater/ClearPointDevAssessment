using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Api.Models;

namespace TodoList.Api.Interfaces
{
    public interface ITodoItemService
    {
        Task<List<TodoItem>> GetTodoItemsAsync();
        Task<TodoItem> GetTodoItemAsync(Guid id);
        Task UpdateTodoItemAsync(Guid id, TodoItem todoItem);
        Task<int> CreateTodoItemAsync(TodoItem todoItem);
        bool TodoItemIdExists(Guid id);
        bool TodoItemDescriptionExists(string description);
    }
}
