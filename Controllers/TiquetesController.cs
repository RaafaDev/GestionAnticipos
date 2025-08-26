using GestionAnticipos.Data;
using GestionAnticiposApp.Data;
using GestionAnticiposApp.ViewModels;
using GestionAnticiposApp.Models; // 👈 Importar namespace del enum y modelos
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAnticiposApp.Controllers
{
    public class TiquetesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TiquetesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Tiquetes
        public async Task<IActionResult> Index()
        {
            var tiquetes = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .Where(p => p.Tipo == TipoProcesoVinculado.Tiquete) // ✅ Enum
                .Select(p => new TiquetesVM
                {
                    Id = p.Id,
                    ProcesoVinculadoId = p.Id,
                    Concepto = p.Codigo,
                    Valor = p.Valor,
                    Fecha = p.FechaSolicitud,
                    Estado = p.Estado,
                    AnticipoCodigo = p.Codigo,
                    ContratoCodigo = p.Contrato.Codigo
                })
                .ToListAsync();

            return View(tiquetes);
        }

        // GET: Tiquetes/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Anticipos = await _context.ProcesosVinculados
                .Include(a => a.Contrato)
                .Where(a => a.Tipo == TipoProcesoVinculado.Anticipo
                         && a.Contrato.FechaFin >= DateTime.Now)
                .ToListAsync();

            return View(new TiquetesVM());
        }

        // POST: Tiquetes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TiquetesVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Anticipos = await _context.ProcesosVinculados
                    .Include(a => a.Contrato)
                    .Where(a => a.Tipo == TipoProcesoVinculado.Anticipo
                             && a.Contrato.FechaFin >= DateTime.Now)
                    .ToListAsync();

                return View(model);
            }

            var anticipo = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(p => p.Id == model.ProcesoVinculadoId);

            if (anticipo == null || anticipo.Contrato.FechaFin < DateTime.Now)
            {
                ModelState.AddModelError("", "El anticipo o contrato no está vigente.");
                return View(model);
            }

            // Guardar tiquete como un nuevo proceso vinculado
            var tiquete = new ProcesosVinculados
            {
                Codigo = $"TQ-{DateTime.Now:yyyyMMddHHmmss}",
                ContratoId = anticipo.ContratoId,
                Estado = "Pendiente",
                FechaSolicitud = model.Fecha,
                Funcionario = anticipo.Funcionario,
                Autorizador = anticipo.Autorizador,
                Tipo = TipoProcesoVinculado.Tiquete, // ✅ Enum
                Valor = model.Valor
            };

            _context.ProcesosVinculados.Add(tiquete);
            await _context.SaveChangesAsync();

            // Guardar documentos asociados
            if (model.Archivos != null && model.Archivos.Count > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                foreach (var file in model.Archivos)
                {
                    if (file.Length > 0)
                    {
                        var filePath = Path.Combine(uploads, file.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var doc = new Documentos
                        {
                            Archivo = $"/uploads/{file.FileName}", // ✅ Solo usamos Archivo
                            ProcesoVinculadoId = tiquete.Id        // ✅ Relación con el tiquete
                        };

                        _context.Documentos.Add(doc);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Tiquetes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tiquete = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .Include(p => p.Documentos)
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == TipoProcesoVinculado.Tiquete);

            if (tiquete == null)
                return NotFound();

            var vm = new TiquetesVM
            {
                Id = tiquete.Id,
                ProcesoVinculadoId = tiquete.Id,
                Concepto = tiquete.Codigo,
                Valor = tiquete.Valor,
                Fecha = tiquete.FechaSolicitud,
                Estado = tiquete.Estado,
                AnticipoCodigo = tiquete.Codigo,
                ContratoCodigo = tiquete.Contrato.Codigo,
                Documentos = tiquete.Documentos.Select(d => d.Archivo).ToList() // ✅ Solo Archivo
            };

            return View(vm);
        }
    }
}
