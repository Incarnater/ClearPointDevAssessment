using Xunit;
using System.Threading.Tasks;
using TodoList.Api.Data;
using TodoList.Api.Services;
using System;
using TodoList.Api.Models;
using TodoList.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Exceptions;
using System.Collections.Generic;

namespace TodoList.Api.UnitTests.Services
{
    public class TodoItemServiceTests : IDisposable
    {
        private readonly TodoContext contextMemory;
        private readonly ITodoItemService _todoItemService;

        public TodoItemServiceTests()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TodoContext>();
            optionsBuilder.UseInMemoryDatabase("TodoItemTestDatabase");
            contextMemory = new TodoContext(optionsBuilder.Options);
            contextMemory.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _todoItemService = new TodoItemService(contextMemory);
        }

        public void Dispose()
        {
            if (contextMemory != null)
            {
                contextMemory.Dispose();
            }
        }


        [Fact]
        public async void GetTodoItemsAsync_NoTodoItems_ReturnsNull()
        {
            contextMemory.Database.EnsureDeleted();
            var todoItemList = await _todoItemService.GetTodoItemsAsync();

            Assert.Empty(todoItemList);
        }

        [Fact]
        public async Task GetTodoItemsAsync_IfExists_ReturnsList()
        {
            var todoItemdtoList = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid(), Description = "Wash dishes", IsCompleted = false },
                new TodoItem { Id = Guid.NewGuid(), Description = "Mow Lawn", IsCompleted = false },
                new TodoItem { Id = Guid.NewGuid(), Description = "Restock food", IsCompleted = false }
            };

            foreach (var todoItemdto in todoItemdtoList)
            {
                await _todoItemService.CreateTodoItemAsync(todoItemdto);
            }

            var todoItemList = await _todoItemService.GetTodoItemsAsync();

            Assert.NotNull(todoItemList);
            Assert.IsType<List<TodoItem>>(todoItemList);
        }

        [Fact]
        public async Task GetTodoItemAsync_IfExists_ReturnsSameTodoItem()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "Clean sink";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = false
            };
            await _todoItemService.CreateTodoItemAsync(todoItemdto);

            var todoItem = await _todoItemService.GetTodoItemAsync(todoItemId);

            Assert.Equal(todoItemId, todoItem.Id);
            Assert.Equal(todoItemDescription, todoItem.Description);
            Assert.False(todoItem.IsCompleted);
        }

        [Fact]
        public async Task UpdateTodoItemAsync_IfExists_ReturnsUpdatedTodoItem()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "Break dishes";
            var todoItemIsCompleted = false;
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = todoItemIsCompleted
            };
            await _todoItemService.CreateTodoItemAsync(todoItemdto);

            var todoItemNewDescription = "Replace dishes";
            todoItemdto.Description = todoItemNewDescription;

            await _todoItemService.UpdateTodoItemAsync(todoItemId, todoItemdto);

            Assert.Equal(todoItemdto.Id, todoItemId);
            Assert.Equal(todoItemdto.Description, todoItemNewDescription);
            Assert.NotEqual(todoItemdto.Description, todoItemDescription);
            Assert.Equal(todoItemdto.IsCompleted, todoItemIsCompleted);
        }

        [Fact]
        public async Task UpdateTodoItemAsync_IfGuidNotFound_ReturnsNotFoundException()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemNewDescription = "Hide dishes";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemNewDescription,
                IsCompleted = false
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _todoItemService.UpdateTodoItemAsync(todoItemId, todoItemdto));
            Assert.Equal("Guid not found", ex.Message);
        }

        [Fact]
        public async Task UpdateTodoItemAsync_IfGuidInvalid_ReturnsBadRequestException()
        {
            var newTodoItemId = Guid.NewGuid();
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "Check Battery";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = false
            };

            await _todoItemService.CreateTodoItemAsync(todoItemdto);
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _todoItemService.UpdateTodoItemAsync(newTodoItemId, todoItemdto));
            Assert.Equal("Invalid Id value supplied", ex.Message);
        }

        [Fact]
        public async Task CreateTodoItem_IfSuccessful_ReturnsSameTodoItem()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "Clean Shed";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = false
            };

            await _todoItemService.CreateTodoItemAsync(todoItemdto);

            var todoItem = await _todoItemService.GetTodoItemAsync(todoItemId);
            Assert.Equal(todoItemId, todoItem.Id);
            Assert.Equal(todoItemDescription, todoItem.Description);
            Assert.False(todoItem.IsCompleted);
        }

        [Fact]
        public async Task CreateTodoItem_LacksDescription_ReturnsBadRequestException()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = false
            };


            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _todoItemService.CreateTodoItemAsync(todoItemdto));
            Assert.Equal("Description is required", ex.Message);
        }

        [Fact]
        public async Task CreateTodoItem_PreExistingDescription_ReturnsBadRequestException()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "Mop Floors";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = false
            };

            await _todoItemService.CreateTodoItemAsync(todoItemdto);

            var newTodoItemdto = new TodoItem
            {
                Id = Guid.NewGuid(),
                Description = todoItemDescription,
                IsCompleted = false
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _todoItemService.CreateTodoItemAsync(todoItemdto));
            Assert.Equal("Description already exists", ex.Message);
        }

        [Fact]
        public async Task TodoItemIdExists_IfExists_ReturnsTrue()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "Paint Fence";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = false
            };
            await _todoItemService.CreateTodoItemAsync(todoItemdto);

            Assert.True(_todoItemService.TodoItemIdExists(todoItemId));

        }

        [Fact]
        public void TodoItemIdExists_IfNotExists_ReturnsFalse()
        {
            var todoItemId = Guid.NewGuid();

            Assert.False(_todoItemService.TodoItemIdExists(todoItemId));

        }

        [Fact]
        public async Task TodoItemDescriptionExists_IfExists_ReturnsTrue()
        {
            var todoItemId = Guid.NewGuid();
            var todoItemDescription = "Paint House";
            var todoItemdto = new TodoItem
            {
                Id = todoItemId,
                Description = todoItemDescription,
                IsCompleted = false
            };
            await _todoItemService.CreateTodoItemAsync(todoItemdto);

            Assert.True(_todoItemService.TodoItemDescriptionExists(todoItemDescription));
        }

        [Fact]
        public void TodoItemDescriptionExists_IfNotExists_ReturnsFalse()
        {
            var todoItemDescription = "Wash House";

            Assert.False(_todoItemService.TodoItemDescriptionExists(todoItemDescription));
        }
    }
}
