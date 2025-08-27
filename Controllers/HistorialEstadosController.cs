using GestionAnticipos.Data;
using GestionAnticipos.Models;
using GestionAnticiposApp.Data;
using GestionAnticiposApp.Models;
using GestionAnticiposApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GestionAnticiposApp.Controllers
{
    [Authorize]
    public class HistorialEstadosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialEstadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            int pageSize = 5;
            var logsQuery = _context.Logs
                .Include(l => l.ProcesoVinculado)
                .Where(l =>
                    l.Campo == "Estado" &&
                    l.ProcesoVinculado.Tipo == 0 // Solo anticipos
                )
                .OrderByDescending(l => l.Fecha);

            var paginatedLogs = await PaginatedList<Log>.CreateAsync(logsQuery, pageIndex, pageSize);
            return View(paginatedLogs);
        }
    }
}

//.Nivel == "Edit" &&