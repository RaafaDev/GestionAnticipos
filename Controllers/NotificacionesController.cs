using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAnticipos.Data;
using GestionAnticiposApp.Models;

namespace GestionAnticiposApp.Controllers
{
    public class NotificacionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificacionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notificaciones
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Notificaciones.Include(n => n.ProcesoVinculado);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Notificaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificaciones = await _context.Notificaciones
                .Include(n => n.ProcesoVinculado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificaciones == null)
            {
                return NotFound();
            }

            return View(notificaciones);
        }

        // GET: Notificaciones/Create
        public IActionResult Create()
        {
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id");
            return View();
        }

        // POST: Notificaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Origen,Tipo,Mensaje,ProcesoVinculadoId")] Notificaciones notificaciones)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notificaciones);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id", notificaciones.ProcesoVinculadoId);
            return View(notificaciones);
        }

        // GET: Notificaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificaciones = await _context.Notificaciones.FindAsync(id);
            if (notificaciones == null)
            {
                return NotFound();
            }
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id", notificaciones.ProcesoVinculadoId);
            return View(notificaciones);
        }

        // POST: Notificaciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Origen,Tipo,Mensaje,ProcesoVinculadoId")] Notificaciones notificaciones)
        {
            if (id != notificaciones.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notificaciones);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificacionesExists(notificaciones.Id))
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
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id", notificaciones.ProcesoVinculadoId);
            return View(notificaciones);
        }

        // GET: Notificaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificaciones = await _context.Notificaciones
                .Include(n => n.ProcesoVinculado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificaciones == null)
            {
                return NotFound();
            }

            return View(notificaciones);
        }

        // POST: Notificaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notificaciones = await _context.Notificaciones.FindAsync(id);
            if (notificaciones != null)
            {
                _context.Notificaciones.Remove(notificaciones);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificacionesExists(int id)
        {
            return _context.Notificaciones.Any(e => e.Id == id);
        }
    }
}
