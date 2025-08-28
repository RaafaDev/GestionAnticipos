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
        private readonly LoggerHelper _loggerHelper;

        public HistorialEstadosController(ApplicationDbContext context, LoggerHelper loggerHelper)
        {
            _context = context;
            _loggerHelper = loggerHelper;
        }

        // GET: HistorialEstados
        public async Task<IActionResult> Index(string searchCodigo, int pageIndex = 1, int pageSize = 10)
        {
            _loggerHelper.LogInfo($"Ingreso a la vista de historial de estados por usuario {User.Identity.Name}");

            var query = _context.Logs
                .Include(l => l.ProcesoVinculado)
                .Where(l => l.Campo == "Estado") 
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchCodigo))
            {
                query = query.Where(l => l.ProcesoVinculado.Codigo.Contains(searchCodigo));
                _loggerHelper.LogInfo($"Búsqueda en historial de estados por código: {searchCodigo} por usuario {User.Identity.Name}");
            }

            var paginatedList = await PaginatedList<Log>.CreateAsync(query.OrderByDescending(l => l.Fecha), pageIndex, pageSize);

            ViewData["searchCodigo"] = searchCodigo;
            return View(paginatedList);
        }

        // GET: HistorialEstados/Details/5
        public async Task<IActionResult> Details(int id)
        {
            _loggerHelper.LogInfo($"Ingreso a la vista de detalles de historial de estado {id} por usuario {User.Identity.Name}");

            var log = await _context.Logs
                .Include(l => l.ProcesoVinculado)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (log == null)
                return NotFound();

            return View(log);
        }
    }
}