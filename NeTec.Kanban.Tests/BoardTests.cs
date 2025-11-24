using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NeTec.Kanban.Domain.Entities;
using Microsoft.AspNetCore.Mvc.ViewFeatures; 
using NeTec.Kanban.Domain.Entities.ViewModel;
using NeTec.Kanban.Infrastructure.Data;
using NeTec.Kanban.Web.Controllers;
using System.Security.Claims;
using Xunit;

namespace NeTec.Kanban.Tests
{
    public class BoardTests
    {
        // HILFSMETHODE 1: Erstellt eine frische Fake-Datenbank für jeden Test
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Eindeutiger Name pro Test
                .Options;

            return new ApplicationDbContext(options);
        }

        // HILFSMETHODE 2: Erstellt einen Fake-Benutzer ("Mock")
        // Damit simulieren wir, dass jemand eingeloggt ist.
        private UserManager<ApplicationUser> GetFakeUserManager(string userId)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            // Wenn der Controller fragt "Wer bin ich?", antworten wir mit der userId
            mgr.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            return mgr.Object;
        }

        [Fact]
        public async Task CreateBoard_ShouldSaveToDatabase()
        {
            // 1. ARRANGE (Vorbereiten)
            var context = GetInMemoryContext();
            var userManager = GetFakeUserManager("user-123");
            var controller = new BoardController(context, userManager);

            controller.TempData = new Mock<ITempDataDictionary>().Object;

            var model = new CreateBoardViewModel
            {
                Titel = "Test Board",
                Description = "Ein Unit-Test Board"
            };

            // 2. ACT (Machen)
            await controller.Create(model);

            // 3. ASSERT (Prüfen)
            var savedBoard = await context.Boards.FirstOrDefaultAsync(b => b.Titel == "Test Board");

            Assert.NotNull(savedBoard);                   // Wurde es gefunden?
            Assert.Equal("user-123", savedBoard.UserId);  // Stimmt der User?
            Assert.Equal(3, savedBoard.Columns.Count);    // Wurden die 3 Standard-Spalten angelegt?
        }

        [Fact]
        public void Board_ShouldInitializeWithEmptyColumns()
        {
            // Testet, ob unser Konstruktor die Listen richtig initialisiert (damit nichts abstürzt)
            var board = new Board();

            Assert.NotNull(board.Columns);
            Assert.Empty(board.Columns);
        }

        [Fact]
        public async Task Delete_ShouldRemoveBoard_WhenUserIsOwner()
        {
            // 1. ARRANGE
            var userId = "chef-1";
            var db = GetInMemoryContext();

            // Wir legen ein Board an, das wir gleich löschen wollen
            var board = new Board
            {
                Id = 99,
                Titel = "Zu löschendes Board",
                UserId = userId
            };
            db.Boards.Add(board);
            await db.SaveChangesAsync();

            // Controller vorbereiten (mit dem richtigen User eingeloggt)
            var controller = new BoardController(db, GetFakeUserManager(userId));

            controller.TempData = new Mock<ITempDataDictionary>().Object;

            // 2. ACT
            var result = await controller.Delete(99);

            // 3. ASSERT
            // Board muss weg sein
            var deletedBoard = await db.Boards.FindAsync(99);
            Assert.Null(deletedBoard);

            // Sollte Redirect zur Index sein
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}