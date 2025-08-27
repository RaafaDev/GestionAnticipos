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
    public class ProcesosVinculadosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProcesosVinculadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProcesosVinculados
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProcesosVinculados.Include(p => p.Contrato);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ProcesosVinculados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procesosVinculados = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (procesosVinculados == null)
            {
                return NotFound();
            }

            return View(procesosVinculados);
        }

        // GET: ProcesosVinculados/Create
        public IActionResult Create()
        {
            ViewData["ContratoId"] = new SelectList(_context.Contratos, "Id", "Id");
            return View();
        }

        // POST: ProcesosVinculados/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Estado,Funcionario,FechaSolicitud,Autorizador,Tipo,Valor,ContratoId")] ProcesosVinculados procesosVinculados)
        {
            if (ModelState.IsValid)
            {
                _context.Add(procesosVinculados);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContratoId"] = new SelectList(_context.Contratos, "Id", "Id", procesosVinculados.ContratoId);
            return View(procesosVinculados);
        }

        // GET: ProcesosVinculados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procesosVinculados = await _context.ProcesosVinculados.FindAsync(id);
            if (procesosVinculados == null)
            {
                return NotFound();
            }
            ViewData["ContratoId"] = new SelectList(_context.Contratos, "Id", "Id", procesosVinculados.ContratoId);
            return View(procesosVinculados);
        }

        // POST: ProcesosVinculados/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Estado,Funcionario,FechaSolicitud,Autorizador,Tipo,Valor,ContratoId")] ProcesosVinculados procesosVinculados)
        {
            if (id != procesosVinculados.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(procesosVinculados);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcesosVinculadosExists(procesosVinculados.Id))
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
            ViewData["ContratoId"] = new SelectList(_context.Contratos, "Id", "Id", procesosVinculados.ContratoId);
            return View(procesosVinculados);
        }

        // GET: ProcesosVinculados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procesosVinculados = await _context.ProcesosVinculados
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (procesosVinculados == null)
            {
                return NotFound();
            }

            return View(procesosVinculados);
        }

        // POST: ProcesosVinculados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var procesosVinculados = await _context.ProcesosVinculados.FindAsync(id);
            if (procesosVinculados != null)
            {
                _context.ProcesosVinculados.Remove(procesosVinculados);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProcesosVinculadosExists(int id)
        {
            return _context.ProcesosVinculados.Any(e => e.Id == id);
        }
    }
}
