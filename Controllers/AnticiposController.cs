using GestionAnticipos.Data;
using GestionAnticiposApp.Data;
using GestionAnticiposApp.Models;
using GestionAnticiposApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAnticipos.Models;

namespace GestionAnticiposApp.Controllers
{
    public class AnticiposController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LoggerHelper _loggerHelper;

        public AnticiposController(ApplicationDbContext context, LoggerHelper loggerHelper)
        {
            _context = context;
            _loggerHelper = loggerHelper;
        }

        // GET: Anticipos
        public async Task<IActionResult> Index(string? FechaSolicitud, string? Estado, string? Codigo, int pageIndex = 1)
        {
            int pageSize = 2;

            var query = _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .AsQueryable();

            if (!string.IsNullOrEmpty(FechaSolicitud))
            {
                if (DateTime.TryParse(FechaSolicitud, out var fecha))
                {
                    query = query.Where(p => p.FechaSolicitud.Date == fecha.Date);
                }
            }

            if (!string.IsNullOrEmpty(Estado))
            {
                query = query.Where(p => p.Estado.Contains(Estado));
            }

            if (!string.IsNullOrEmpty(Codigo))
            {
                query = query.Where(p => p.Codigo.Contains(Codigo));
            }

            ViewData["FechaSolicitud"] = FechaSolicitud;
            ViewData["Estado"] = Estado;
            ViewData["CodigoContra"] = Codigo;

            var paginatedList = await PaginatedList<ProcesosVinculados>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió al listado de anticipos.");

            return View(paginatedList);
        }

        // GET: Anticipos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);



        // ===============================
        // MÉTODOS PRIVADOS DE MAPEADO
        // ===============================
        private ProcesosVinculados MapToEntity(AnticipoVM vm, int contratoId)
        {
            return new ProcesosVinculados
            {
                Id = vm.Id,
                Codigo = vm.Codigo,
                Estado = vm.Estado,
                FechaSolicitud = vm.FechaSolicitud,
                Valor = vm.Valor,
                Tipo = 0,               // fijo
                ContratoId = contratoId,
                Funcionario = User.Identity?.Name ?? "Desconocido",
                Autorizador = ""                 // lo puedes rellenar luego si aplica
            };
        }

        private AnticipoVM MapToVM(ProcesosVinculados entity)
        {
            ViewData["FechaSolicitud"] = entity.FechaSolicitud;
            ViewData["CodigoContra"] = entity.Codigo;
            ViewData["Estado"] = entity.Estado;
            return new AnticipoVM
            {
                Id = entity.Id,
                Codigo = entity.Codigo,
                Estado = entity.Estado,
                FechaSolicitud = entity.FechaSolicitud,
                Valor = entity.Valor,
                Funcionario = entity.Funcionario, // <-- ASIGNACIÓN CORRECTA
                Comentarios = "",         
                PuedeAprobar = false      
            };
        }

        // GET: Anticipos/Create
        public IActionResult Create(int contratoId)
        {
            var vm = new AnticipoVM();
            ViewBag.ContratoId = contratoId;
            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió a la vista de creación de anticipo para el contrato {contratoId}.");
            return View(vm);
        }

        // POST: Anticipos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnticipoVM vm, int contratoId, List<IFormFile> Documentos)
        {

            var errores = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errores)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ContratoId = contratoId;
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó crear un anticipo pero la validación falló para el contrato {contratoId}.");
                return View(vm);
            }

            
            var entity = MapToEntity(vm, contratoId);

            _context.ProcesosVinculados.Add(entity);
            await _context.SaveChangesAsync(); // primero guardamos para tener el Id

            // 📂 Si hay documentos subidos
            if (Documentos != null && Documentos.Any())
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                foreach (var file in Documentos)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var nuevoDocumento = new Documentos
                        {
                            Nombre = file.FileName, // nombre original
                            Tipo = file.ContentType,
                            Archivo = "/uploads/" + fileName, // ruta accesible
                            FechaCreacion = DateTime.Now,
                            FechaModificacion = DateTime.Now,
                            ProcesoVinculadoId = entity.Id
                        };

                        _context.Documentos.Add(nuevoDocumento);
                    }
                }

                await _context.SaveChangesAsync();
            }

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} creó un anticipo con código {entity.Codigo} para el contrato {contratoId}.");

            return RedirectToAction("Details", "Contratos", new { id = contratoId });
        }



        // GET: Anticipos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);
            if (entity == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó editar un anticipo inexistente (Id: {id}).");
                return NotFound();
            }

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió a la edición del anticipo con Id: {id}.");

            var vm = MapToVM(entity);
            ViewBag.ContratoId = entity.ContratoId;
            SetTipoViewData();
            return View(vm);
        }

        // POST: Anticipos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Aprobador, Administrador")]
        public async Task<IActionResult> Edit(int id, AnticipoVM vm, int contratoId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ContratoId = contratoId;
                SetTipoViewData();
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó editar un anticipo pero la validación falló (Id: {id}).");
                return View(vm);
            }

            var entity = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);
            if (entity == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó editar un anticipo inexistente (Id: {id}).");
                return NotFound();
            }

            // Guardar valores anteriores para el log
            var original = new ProcesosVinculados
            {
                Id = entity.Id,
                Codigo = entity.Codigo,
                Estado = entity.Estado,
                FechaSolicitud = entity.FechaSolicitud,
                Valor = entity.Valor,
                Funcionario = entity.Funcionario,
                Autorizador = entity.Autorizador,
                Tipo = entity.Tipo,
                ContratoId = entity.ContratoId
            };

            // Actualizar datos mapeados
            entity.Codigo = vm.Codigo;
            entity.Estado = vm.Estado;
            entity.FechaSolicitud = vm.FechaSolicitud;
            entity.Valor = vm.Valor;

            _context.Update(entity);
            await _context.SaveChangesAsync();

            // Log de cambios automáticos
            _loggerHelper.LogEntityChanges(original, entity);

            return RedirectToAction("Details", "Contratos", new { id = contratoId });
        }

        // GET: Anticipos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);
            if (entity == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó eliminar un anticipo inexistente (Id: {id}).");
                return NotFound();
            }

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió a la vista de eliminación del anticipo con Id: {id}.");

            var vm = MapToVM(entity);
            ViewBag.ContratoId = entity.ContratoId;
            SetTipoViewData();
            return View(vm);
        }

        // POST: Anticipos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int contratoId)
        {
            var entity = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);
            if (entity == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó eliminar un anticipo inexistente (Id: {id}).");
                return NotFound();
            }

            _context.ProcesosVinculados.Remove(entity);
            await _context.SaveChangesAsync();

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} eliminó el anticipo con Id: {id} para el contrato {contratoId}.");

            return RedirectToAction("Details", "Contratos", new { id = contratoId });
        }
          // Anticipos/Delete/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);

            if (entity == null) return NotFound();

            var vm = MapToVM(entity);

    

            ViewBag.ContratoId = entity.ContratoId;

            return View(vm);
        }

    }
}