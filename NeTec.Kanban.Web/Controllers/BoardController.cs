using Microsoft.AspNetCore.Mvc;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;

namespace NeTec.Kanban.Web.Controllers
{
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BoardController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Board> boards = _db.Boards;
            return View(boards);
        }
    }
}
