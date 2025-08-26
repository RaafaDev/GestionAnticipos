using GestionAnticipos.Data;
using GestionAnticiposApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestionAnticipos.Models;

namespace GestionAnticiposApp.Controllers
{
    public class ContratosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LoggerHelper _loggerHelper;

        public ContratosController(ApplicationDbContext context, LoggerHelper loggerHelper)
        {
            _context = context;
            _loggerHelper = loggerHelper;
        }

        // GET: Contratos
        public async Task<IActionResult> Index(string searchCodigo, int pageIndex = 1)
        {
            ViewData["searchCodigo"] = searchCodigo;
            int pageSize = 4;

            var contratos = from c in _context.Contratos
                            select c;

            if (!string.IsNullOrEmpty(searchCodigo))
            {
                contratos = contratos.Where(c => c.Codigo.Contains(searchCodigo));
            }

            var paginatedList = await PaginatedList<Contratos>.CreateAsync(contratos.AsNoTracking(), pageIndex, pageSize);

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió al listado de contratos.");

            return View(paginatedList);
        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó acceder a detalles de un contrato inexistente (Id: {id}).");
                _loggerHelper.LogError($"Acceso fallido a detalles de contrato. Id nulo.");
                return NotFound();
            }

            var contratos = await _context.Contratos
                .Include(m => m.ProcesosVinculados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contratos == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó acceder a detalles de un contrato inexistente (Id: {id}).");
                _loggerHelper.LogError($"Acceso fallido a detalles de contrato. Contrato no encontrado.");
                return NotFound();
            }

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió a los detalles del contrato con código {contratos.Codigo}.");

            return View(contratos);
        }

        // GET: Contratos/Create
        public IActionResult Create()
        {
            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió a la vista de creación de contrato.");
            return View();
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Estado,Servicio,FechaInicio,FechaFin,Empresa,Referencia")] Contratos contratos)
        {
            var errores = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errores)
            {
                Console.WriteLine(error.ErrorMessage);
                _loggerHelper.LogError($"Error de validación al crear contrato: {error.ErrorMessage}");
            }

            if (!ModelState.IsValid)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó crear un contrato pero la validación falló.");
                return View(contratos);
            }

            _context.Add(contratos);
            await _context.SaveChangesAsync();
            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} creó el contrato con código {contratos.Codigo}.");

            return RedirectToAction(nameof(Index));
        }

        // GET: Contratos/Edit/5
        [Authorize(Roles = "Aprobador, Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó editar un contrato inexistente (Id: {id}).");
                _loggerHelper.LogError($"Acceso fallido a edición de contrato. Id nulo.");
                return NotFound();
            }

            var contratos = await _context.Contratos.FindAsync(id);
            if (contratos == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó editar un contrato inexistente (Id: {id}).");
                _loggerHelper.LogError($"Acceso fallido a edición de contrato. Contrato no encontrado.");
                return NotFound();
            }
            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió a la edición del contrato con código {contratos.Codigo}.");
            return View(contratos);
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Estado,Servicio,FechaInicio,FechaFin,Empresa,Referencia")] Contratos contratos)
        {
            if (id != contratos.Id)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó editar un contrato con id no coincidente (Id: {id}).");
                _loggerHelper.LogError($"Id no coincide al editar contrato.");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó editar un contrato pero la validación falló (Id: {id}).");
                _loggerHelper.LogError($"Error de validación al editar contrato.");
                return View(contratos);
            }

            try
            {
                var original = await _context.Contratos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == contratos.Id);
                _context.Update(contratos);
                await _context.SaveChangesAsync();

                // Solo aquí: log de cambios campo a campo
                if (original != null)
                    _loggerHelper.LogEntityChanges(original, contratos);

                _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} editó el contrato con código {contratos.Codigo}.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _loggerHelper.LogError($"Error de concurrencia al editar el contrato (Id: {contratos.Id}): {ex.Message}");
                if (!ContratosExists(contratos.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Contratos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó eliminar un contrato inexistente (Id: {id}).");
                _loggerHelper.LogError($"Acceso fallido a eliminación de contrato. Id nulo.");
                return NotFound();
            }

            var contratos = await _context.Contratos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contratos == null)
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó eliminar un contrato inexistente (Id: {id}).");
                _loggerHelper.LogError($"Acceso fallido a eliminación de contrato. Contrato no encontrado.");
                return NotFound();
            }

            _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} accedió a la eliminación del contrato con código {contratos.Codigo}.");
            return View(contratos);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contratos = await _context.Contratos.FindAsync(id);
            if (contratos != null)
            {
                _context.Contratos.Remove(contratos);
                _loggerHelper.LogInfo($"El usuario {User.Identity?.Name ?? "Desconocido"} eliminó el contrato con código {contratos.Codigo}.");
            
            }
            else
            {
                _loggerHelper.LogWarning($"El usuario {User.Identity?.Name ?? "Desconocido"} intentó eliminar un contrato inexistente (Id: {id}).");
                _loggerHelper.LogError($"Intento fallido de eliminación de contrato.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContratosExists(int id)
        {
            return _context.Contratos.Any(e => e.Id == id);
        }
    }
}