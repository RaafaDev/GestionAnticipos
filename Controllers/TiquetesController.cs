using GestionAnticipos.Data;
using GestionAnticipos.Models;
using GestionAnticiposApp.Data;
using GestionAnticiposApp.Models; // 👈 Importar namespace del enum y modelos
using GestionAnticiposApp.Models.ViewModels;
using GestionAnticiposApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestionAnticiposApp.Controllers
{
    [Authorize]
    public class TiquetesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TiquetesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ✅ LISTAR TIQUETES
        public async Task<IActionResult> Index()
        {
            var tiquetes = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .Where(p => p.Tipo == TipoProcesoVinculado.Tiquete) // Solo tiquetes
                .Select(p => new TiquetesVM
                {
                    Id = p.Id,
                    ProcesoVinculadoId = p.Id,
                    ContratoId = p.Contrato != null ? p.Contrato.Id : 0,
                    Concepto = p.Comentarios, // 👈 Usamos "Comentarios" como concepto
                    Valor = p.Valor,
                    Fecha = p.FechaSolicitud,
                    Estado = p.Estado
                })
                .ToListAsync();

            return View(tiquetes);
        }

        // GET: Tiquetes/Create
        public async Task<IActionResult> Create(int contratoId)
        {
            // ✅ Traer anticipos SOLO del contrato actual
            var anticipos = await _context.ProcesosVinculados
                .Include(a => a.Contrato)
                .Where(a => a.Tipo == TipoProcesoVinculado.Anticipo
                         && a.ContratoId == contratoId)
                .Select(a => new
                {
                    a.Id,
                    Codigo = $"{a.Codigo} | Valor: {a.Valor:C0} | Fecha: {a.FechaSolicitud:dd/MM/yyyy}"
                })
                .ToListAsync();

            ViewBag.Anticipos = new SelectList(anticipos, "Id", "Codigo");
            ViewBag.ContratoId = contratoId;

            return View(new TiquetesVM { ContratoId = contratoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TiquetesVM model)
        {
            if (!ModelState.IsValid)
            {
                var anticipos = await _context.ProcesosVinculados
                    .Where(a => a.Tipo == TipoProcesoVinculado.Anticipo
                             && a.ContratoId == model.ContratoId)
                    .Select(a => new
                    {
                        a.Id,
                        Codigo = $"{a.Codigo} | Valor: {a.Valor:C0} | Fecha: {a.FechaSolicitud:dd/MM/yyyy}"
                    })
                    .ToListAsync();

                ViewBag.Anticipos = new SelectList(anticipos, "Id", "Codigo", model.ProcesoVinculadoId);
                ViewBag.ContratoId = model.ContratoId;

                return View(model);
            }

            // Buscar el anticipo relacionado
            var anticipo = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(p => p.Id == model.ProcesoVinculadoId);

            if (anticipo == null || anticipo.ContratoId != model.ContratoId)
            {
                ModelState.AddModelError("", "El anticipo no pertenece al contrato actual.");
                return View(model);
            }

            // Crear el tiquete ligado al contrato
            var tiquete = new ProcesosVinculados
            {
                Codigo = $"TQ-{DateTime.Now:yyyyMMddHHmmss}",
                ContratoId = model.ContratoId,
                Estado = "Pendiente",
                FechaSolicitud = model.Fecha,
                Funcionario = anticipo.Funcionario,
                Autorizador = anticipo.Autorizador,
                Tipo = TipoProcesoVinculado.Tiquete,
                Valor = model.Valor,
                Comentarios = model.Concepto
            };

            _context.ProcesosVinculados.Add(tiquete);
            await _context.SaveChangesAsync();

            // Guardar documentos
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
                            Archivo = $"/uploads/{file.FileName}",
                            ProcesoVinculadoId = tiquete.Id
                        };

                        _context.Documentos.Add(doc);
                    }
                }

                await _context.SaveChangesAsync();
            }

            // ✅ redirigir de nuevo al contrato
            return RedirectToAction("Details", "Contratos", new { id = model.ContratoId });
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
                Concepto = tiquete.Comentarios,
                Valor = tiquete.Valor,
                Fecha = tiquete.FechaSolicitud,
                Estado = tiquete.Estado,
                ContratoId = tiquete.ContratoId,
                Documentos = tiquete.Documentos
                    .Select(d => d.Archivo)
                    .ToList()
            };

            return View(vm);
        }
    }
}
