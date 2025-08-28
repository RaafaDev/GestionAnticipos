using GestionAnticipos.Data;
using GestionAnticipos.Models;
using GestionAnticiposApp.Data;
using GestionAnticiposApp.Models; // Asegúrate de tener el using para PaginatedList
using GestionAnticiposApp.Models.ViewModels;
using GestionAnticiposApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity;

namespace GestionAnticiposApp.Controllers
{
    [Authorize]
    public class TiquetesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly LoggerHelper _loggerHelper;

        public TiquetesController(ApplicationDbContext context, IWebHostEnvironment env, LoggerHelper loggerHelper)
        {
            _context = context;
            _env = env;
            _loggerHelper = loggerHelper;
        }

        // ✅ LISTAR TIQUETES
        public async Task<IActionResult> Index(
            DateTime? searchFechaSolicitud,
            string searchEstado,
            int? searchContratoId,
            int pageIndex = 1,
            int pageSize = 10)
        {
            _loggerHelper.LogInfo($"Ingreso a la vista de listado de tiquetes por usuario {User.Identity.Name}");

            var query = _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .Where(p => p.Tipo == TipoProcesoVinculado.Tiquete);

            if (searchFechaSolicitud.HasValue)
                query = query.Where(p => p.FechaSolicitud.Date == searchFechaSolicitud.Value.Date);

            if (!string.IsNullOrEmpty(searchEstado))
                query = query.Where(p => p.Estado.Contains(searchEstado));

            if (searchContratoId.HasValue)
                query = query.Where(p => p.ContratoId == searchContratoId.Value);

            var vmQuery = query.Select(p => new TiquetesVM
            {
                Id = p.Id,
                ProcesoVinculadoId = p.Id,
                ContratoId = p.Contrato != null ? p.Contrato.Id : 0,
                Concepto = p.Comentarios,
                Valor = p.Valor,
                Fecha = p.FechaSolicitud,
                Estado = p.Estado
            });

            var paginatedList = await PaginatedList<TiquetesVM>.CreateAsync(vmQuery, pageIndex, pageSize);

            ViewData["searchFechaSolicitud"] = searchFechaSolicitud?.ToString("yyyy-MM-dd");
            ViewData["searchEstado"] = searchEstado;
            ViewData["searchContratoId"] = searchContratoId;

            return View(paginatedList);
        }

        // GET: Tiquetes/Create
        public async Task<IActionResult> Create(int contratoId)
        {
            _loggerHelper.LogInfo($"Ingreso a la vista de creación de tiquete para contrato {contratoId} por usuario {User.Identity.Name}");

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

            // Si hay error de anticipo
            if (anticipo == null || anticipo.ContratoId != model.ContratoId)
            {
                _loggerHelper.LogWarning($"Intento fallido de crear tiquete para contrato {model.ContratoId} con anticipo {model.ProcesoVinculadoId}");
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

            // Después de guardar el tiquete
            _loggerHelper.LogInfo($"Tiquete creado: {tiquete.Codigo} (ID: {tiquete.Id}) por usuario {User.Identity.Name}");

            // ✅ redirigir de nuevo al contrato
            return RedirectToAction("Details", "Contratos", new { id = model.ContratoId });
        }

        // GET: Tiquetes/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            {
                if (id == null) return NotFound();

                var tiquete = await _context.ProcesosVinculados
                    .Include(p => p.Contrato)
                    .Include(p => p.Documentos)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == TipoProcesoVinculado.Tiquete);

                _loggerHelper.LogInfo($"Ingreso a la vista de detalles de tiquete {id} por usuario {User.Identity.Name}");



                if (tiquete == null) return NotFound();


                var vm = new TiquetesVM
                {
                    Id = tiquete.Id,
                    ProcesoVinculadoId = tiquete.Id,
                    ContratoId = tiquete.ContratoId,
                    Concepto = tiquete.Comentarios ?? "",
                    Valor = tiquete.Valor,
                    Fecha = tiquete.FechaSolicitud,
                    Estado = tiquete.Estado,
                    Documentos = tiquete.Documentos
                        .Select(d => d.Archivo) // 👈 usamos Archivo
                        .ToList()
                };

                return View(vm);
            }
        }

        // GET: Tiquetes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            _loggerHelper.LogInfo($"Ingreso a la vista de edición de tiquete {id} por usuario {User.Identity.Name}");

            var tiquete = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == TipoProcesoVinculado.Tiquete);

            if (tiquete == null)
                return NotFound();

            // ✅ Traer anticipos SOLO del contrato actual
            var anticipos = await _context.ProcesosVinculados
                .Include(a => a.Contrato)
                .Where(a => a.Tipo == TipoProcesoVinculado.Anticipo
                         && a.ContratoId == tiquete.ContratoId)
                .Select(a => new
                {
                    a.Id,
                    Codigo = $"{a.Codigo} | Valor: {a.Valor:C0} | Fecha: {a.FechaSolicitud:dd/MM/yyyy}"
                })
                .ToListAsync();

            ViewBag.Anticipos = new SelectList(anticipos, "Id", "Codigo", tiquete.Id);
            ViewBag.ContratoId = tiquete.ContratoId;

            var vm = new TiquetesVM
            {
                Id = tiquete.Id,
                ProcesoVinculadoId = tiquete.Id,
                Concepto = tiquete.Comentarios,
                Valor = tiquete.Valor,
                Fecha = tiquete.FechaSolicitud,
                Estado = tiquete.Estado,
                ContratoId = tiquete.ContratoId
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TiquetesVM model)
        {
            var original = await _context.ProcesosVinculados.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (original == null)
                return NotFound();

            // Actualizar solo los campos necesarios
            original.Valor = model.Valor;
            original.Comentarios = model.Concepto;
            original.FechaSolicitud = model.Fecha;

            _context.Update(original);
            await _context.SaveChangesAsync();

            // Log de cambios campo a campo
            _loggerHelper.LogEntityChanges(original, original);

            _loggerHelper.LogInfo($"Tiquete editado: {original.Codigo} (ID: {original.Id}) por usuario {User.Identity.Name}");

            return RedirectToAction("Details", new { id = original.Id });
        }

        // GET: Tiquetes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            _loggerHelper.LogInfo($"Ingreso a la vista de eliminación de tiquete {id} por usuario {User.Identity.Name}");

            var tiquete = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == TipoProcesoVinculado.Tiquete);

            if (tiquete == null)
                return NotFound();

            return View(tiquete);
        }

        // POST: Tiquetes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tiquete = await _context.ProcesosVinculados.FindAsync(id);
            _context.ProcesosVinculados.Remove(tiquete);
            await _context.SaveChangesAsync();

            _loggerHelper.LogInfo($"Tiquete eliminado: {tiquete.Codigo} (ID: {tiquete.Id}) por usuario {User.Identity.Name}");

            // redirigir a la lista de tiquetes
            return RedirectToAction(nameof(Index));
        }
    }
}
