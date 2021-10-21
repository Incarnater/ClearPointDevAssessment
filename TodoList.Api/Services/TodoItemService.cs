using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Api.Data;
using TodoList.Api.Exceptions;
using TodoList.Api.Interfaces;
using TodoList.Api.Models;

namespace TodoList.Api.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly TodoContext _context;

        public TodoItemService(TodoContext context)
        {
            _context = context;
        }

        public async Task<List<TodoItem>> GetTodoItemsAsync()
        {
            return await _context.TodoItems.Where(x => !x.IsCompleted).ToListAsync();
        }

        public async Task<TodoItem> GetTodoItemAsync(Guid id)
        {
            return await _context.TodoItems.FindAsync(id);
        }

        public async Task UpdateTodoItemAsync(Guid id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                throw new BadRequestException("Invalid Id value supplied");
            }
            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemIdExists(id))
                {
                    throw new NotFoundException("Guid not found");
                }
                else
                {
                    throw;
                }
            }

            return;
        }

        public async Task<int> CreateTodoItemAsync(TodoItem todoItem)
        {
            if (string.IsNullOrEmpty(todoItem?.Description))
            {
                throw new BadRequestException("Description is required");
            }
            else if (TodoItemDescriptionExists(todoItem.Description))
            {
                throw new BadRequestException("Description already exists");
            }

            _context.TodoItems.Add(todoItem);
            return await _context.SaveChangesAsync();
        }


        public bool TodoItemDescriptionExists(string description)
        {
            return _context.TodoItems.Any(x => x.Description.ToLowerInvariant() == description.ToLowerInvariant() && !x.IsCompleted);
        }

        public bool TodoItemIdExists(Guid id)
        {
            return _context.TodoItems.Any(x => x.Id == id);
        }
    }
}
