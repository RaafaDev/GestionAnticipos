using GestionAnticipos.Data;
using GestionAnticiposApp.Data;
using GestionAnticiposApp.Models;
using GestionAnticiposApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAnticiposApp.Controllers
{
    public class AnticiposController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnticiposController(ApplicationDbContext context)
        {
            _context = context;
        }

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

            return View(paginatedList);
        }




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

        private void SetTipoViewData()
        {
            ViewData["Tipo"] = new Dictionary<int, string>
            {
                { 0, "Anticipo" },
                { 1, "Tiquete" },
                { 2, "Legalizacion" }
            };
        }

        // ===============================
        // CRUD EJEMPLO
        // ===============================

        // GET: Anticipos/Create
        public IActionResult Create(int contratoId)
        {
            var vm = new AnticipoVM();
            ViewBag.ContratoId = contratoId; // para pasar al formulario
            return View(vm);
        }



        // POST: Anticipos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnticipoVM vm, int contratoId, List<IFormFile> Documentos)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ContratoId = contratoId;
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

            return RedirectToAction("Details", "Contratos", new { id = contratoId });
        }



        // GET: Anticipos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);
            if (entity == null) return NotFound();

            var vm = MapToVM(entity);
            ViewBag.ContratoId = entity.ContratoId;
            SetTipoViewData();


            return View(vm);
        }

        // POST: Anticipos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnticipoVM vm, int contratoId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ContratoId = contratoId;
                SetTipoViewData();
                return View(vm);
            }

            var entity = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);
            if (entity == null) return NotFound();
      

            // actualizar datos mapeados
            entity.Codigo = vm.Codigo;
            entity.Estado = vm.Estado;
            entity.FechaSolicitud = vm.FechaSolicitud;
            entity.Valor = vm.Valor;

            _context.Update(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Contratos", new { id = contratoId });
        }

        // GET: Anticipos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.ProcesosVinculados
                .FirstOrDefaultAsync(p => p.Id == id && p.Tipo == 0);
            if (entity == null) return NotFound();

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
            if (entity == null) return NotFound();

            _context.ProcesosVinculados.Remove(entity);
            await _context.SaveChangesAsync();

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