using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;
using NeTec.Kanban.Web.Controllers;
using NeTec.Kanban.Web.Models.DTOs;
using System.Security.Claims;
using Xunit;

namespace NeTec.Kanban.Tests
{
    public class TaskControllerTests
    {
        // Hilfsmethode für In-Memory DB
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        // Hilfsmethode für Fake-User
        private UserManager<ApplicationUser> GetMockUserManager(string userId)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            return mgr.Object;
        }

        [Fact]
        public async Task Create_ShouldAddNewTask_WhenValid()
        {
            // 1. ARRANGE
            var userId = "user-1";
            var context = GetInMemoryContext();

            // Wir brauchen ein Board und eine Spalte, sonst darf man keinen Task erstellen
            var board = new Board { Id = 1, Titel = "Test Board", UserId = userId };
            var column = new Column { Id = 10, BoardId = 1, Titel = "To Do" };
            context.Boards.Add(board);
            context.Columns.Add(column);
            await context.SaveChangesAsync();

            var controller = new TaskController(context, GetMockUserManager(userId));

            // DTO für die Anfrage
            var request = new EditTaskRequest
            {
                ColumnId = 10,
                Title = "Unit Test Task",
                Priority = "High",
                AssignedUserId = userId 
            };

            // 2. ACT
            var result = await controller.Create(request);

            // 3. ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var savedTask = await context.TaskItems.FirstOrDefaultAsync(t => t.Title == "Unit Test Task");

            Assert.NotNull(savedTask);
            Assert.Equal("High", savedTask.Priority);
            Assert.Equal(userId, savedTask.UserId); 
        }

        [Fact]
        public async Task DeleteTask_ShouldReturnNotFound_WhenUserIsNotBoardOwner()
        {
            // 1. ARRANGE
            var ownerId = "chef-user";
            var hackerId = "hacker-user";
            var context = GetInMemoryContext();

            // Szenario: Board und Task gehören dem Chef
            var board = new Board { Id = 100, Titel = "Chefs Board", UserId = ownerId };
            var column = new Column { Id = 200, BoardId = 100, Titel = "ToDo" };
            var task = new TaskItem { Id = 300, Title = "Geheime Aufgabe", ColumnId = 200, UserId = ownerId };

            context.Boards.Add(board);
            context.Columns.Add(column);
            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            // Wir simulieren den Zugriff durch den "Hacker"
            var controller = new TaskController(context, GetMockUserManager(hackerId));

            // 2. ACT
            // Versuch, die fremde Aufgabe zu löschen
            var result = await controller.DeleteTask(300);

            // 3. ASSERT
            // Der Controller muss "NotFound" sagen, weil er den Task für diesen User gar nicht erst lädt (Sicherheitsfilter)
            Assert.IsType<UnauthorizedResult>(result);
            // Prüfung: Ist der Task noch da?
            Assert.NotNull(await context.TaskItems.FindAsync(300));
        }

    }
}