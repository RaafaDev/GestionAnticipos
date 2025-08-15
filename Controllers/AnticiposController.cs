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
            return new AnticipoVM
            {
                Id = entity.Id,
                Codigo = entity.Codigo,
                Estado = entity.Estado,
                FechaSolicitud = entity.FechaSolicitud,
                Valor = entity.Valor,

                // Estos campos son solo de la vista
                Comentarios = "",         // si vienen de otro lado, agrégalos
                PuedeAprobar = false      // puedes calcularlo según la lógica de negocio
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
        public async Task<IActionResult> Create(AnticipoVM vm, int contratoId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ContratoId = contratoId;
                return View(vm);
            }

            var entity = MapToEntity(vm, contratoId);
            _context.ProcesosVinculados.Add(entity);
            await _context.SaveChangesAsync();

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
    }
}