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

namespace GestionAnticiposApp.Controllers
{
    [Authorize]
    public class AprobacionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AprobacionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Aprobaciones
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Aprobaciones.Include(a => a.ProcesoVinculado);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Aprobaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobaciones = await _context.Aprobaciones
                .Include(a => a.ProcesoVinculado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aprobaciones == null)
            {
                return NotFound();
            }

            return View(aprobaciones);
        }

        // GET: Aprobaciones/Create
        public IActionResult Create()
        {
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id");
            return View();
        }

        // POST: Aprobaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Estado,Mensaje,Fecha,ProcesoVinculadoId")] Aprobaciones aprobaciones)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aprobaciones);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id", aprobaciones.ProcesoVinculadoId);
            return View(aprobaciones);
        }

        // GET: Aprobaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobaciones = await _context.Aprobaciones.FindAsync(id);
            if (aprobaciones == null)
            {
                return NotFound();
            }
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id", aprobaciones.ProcesoVinculadoId);
            return View(aprobaciones);
        }

        // POST: Aprobaciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Estado,Mensaje,Fecha,ProcesoVinculadoId")] Aprobaciones aprobaciones)
        {
            if (id != aprobaciones.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aprobaciones);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AprobacionesExists(aprobaciones.Id))
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
            ViewData["ProcesoVinculadoId"] = new SelectList(_context.ProcesosVinculados, "Id", "Id", aprobaciones.ProcesoVinculadoId);
            return View(aprobaciones);
        }

        // GET: Aprobaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobaciones = await _context.Aprobaciones
                .Include(a => a.ProcesoVinculado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aprobaciones == null)
            {
                return NotFound();
            }

            return View(aprobaciones);
        }

        // POST: Aprobaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aprobaciones = await _context.Aprobaciones.FindAsync(id);
            if (aprobaciones != null)
            {
                _context.Aprobaciones.Remove(aprobaciones);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AprobacionesExists(int id)
        {
            return _context.Aprobaciones.Any(e => e.Id == id);
        }
    }
}
