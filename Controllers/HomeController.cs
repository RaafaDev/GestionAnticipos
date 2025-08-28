using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestionAnticiposApp.Models;
using Microsoft.AspNetCore.Authorization;
using GestionAnticipos.Models; // ?? Importa el namespace de LoggerHelper

namespace GestionAnticiposApp.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly LoggerHelper _loggerHelper;

    public HomeController(LoggerHelper loggerHelper)
    {
        _loggerHelper = loggerHelper;
    }

    public IActionResult Index()
    {
        _loggerHelper.LogInfo($"Ingreso a la vista principal por usuario {User.Identity?.Name ?? "Anónimo"}");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
