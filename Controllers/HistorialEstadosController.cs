using GestionAnticipos.Data;
using GestionAnticiposApp.Data;
using GestionAnticiposApp.Models;
using GestionAnticiposApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAnticipos.Models;
namespace GestionAnticiposApp.Controllers
{
    public class HistorialEstadosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialEstadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            int pageSize = 5; // Puedes ajustar el tamaño de página
            var logsQuery = _context.Logs
                .Where (l =>l.Nivel == "Edit" && l.Campo == "Estado") .OrderByDescending(l => l.Fecha);
            var paginatedLogs = await PaginatedList<Log>.CreateAsync(logsQuery, pageIndex, pageSize);
            return View(paginatedLogs);
        }
    }
}
