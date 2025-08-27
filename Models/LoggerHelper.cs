using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using GestionAnticipos.Data;
using GestionAnticiposApp.Models;

namespace GestionAnticipos.Models;

public class LoggerHelper
{
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _dbContext;

    public LoggerHelper(ILogger<LoggerHelper> logger, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
        GuardarLog("Info", message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
        GuardarLog("Warning", message);
    }

    public void LogError(string message)
    {
        _logger.LogError(message);
        GuardarLog("Error", message);
    }

    public void LogEdit(string usuario, string entidad, object id, string campo, string valorAntes, string valorDespues)
    {
        var message = $"el usuario: {usuario}, Entidad: {entidad}, ID: {id}, Campo: {campo}, Valor Antes: {valorAntes}, Valor Después: {valorDespues}";
        _logger.LogInformation(message);
        GuardarLog("Edit", message, entidad, campo, valorAntes, valorDespues);
    }

    public void LogEntityChanges<T>(T original, T modificado)
    {
        var usuario = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Desconocido";
        var entidad = typeof(T).Name;
        var idProp = typeof(T).GetProperty("Id");
        var id = idProp != null ? idProp.GetValue(modificado) : null;

        foreach (var prop in typeof(T).GetProperties())
        {
            // Ignora propiedades de navegación y colecciones
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                continue;

            var valorAntes = prop.GetValue(original)?.ToString() ?? "";
            var valorDespues = prop.GetValue(modificado)?.ToString() ?? "";

            if (valorAntes != valorDespues)
            {
                GuardarLog(
                    nivel: "Edit",
                    mensaje: $"El usuario {usuario} cambió en {entidad}.{prop.Name} de '{valorAntes}' a '{valorDespues}'",
                    entidad: entidad,
                    campo: prop.Name,
                    valorAntes: valorAntes,
                    valorDespues: valorDespues
                );
            }
        }
    }

    private void GuardarLog(string nivel, string mensaje, string? entidad = null, string? campo = null, string? valorAntes = null, string? valorDespues = null)
    {
        var usuario = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Desconocido";
        var log = new Log
        {
            Fecha = DateTime.Now,
            Usuario = usuario,
            Nivel = nivel,
            Mensaje = mensaje,
            Entidad = entidad,
            Campo = campo,
            ValorAntes = valorAntes,
            ValorDespues = valorDespues
        };
        _dbContext.Logs.Add(log);
        _dbContext.SaveChanges();
    }
}

