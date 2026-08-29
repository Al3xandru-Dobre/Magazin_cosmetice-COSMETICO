using System.Text.Json;
using Magazin_cosmetice_COSMETICO.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Magazin_cosmetice_COSMETICO.Middleware;

/// <summary>
/// Middleware global de exceptii (cerinta de 3p).
///
/// De ce middleware si nu try/catch in fiecare controller?
/// 1. Un singur loc pentru formatul de eroare -> raspunsuri consistente.
/// 2. Controllerele raman curate: 3-5 linii per actiune, zero try/catch.
/// 3. Prinde si exceptiile din straturile de sub controller (service, repository).
///
/// Se inregistreaza PRIMUL in pipeline (vezi Program.cs): pipeline-ul e o
/// matrioska, iar ce e inregistrat primul "invaluie" tot ce urmeaza.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // pasam controlul mai departe in pipeline
        }
        catch (AppException ex)
        {
            // Eroare "asteptata": vina clientului. Log la nivel Warning,
            // mesajul poate fi aratat utilizatorului.
            _logger.LogWarning(ex, "Eroare de aplicatie: {Message}", ex.Message);
            await WriteProblemAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Eroare neasteptata: bug. Log la Error cu stack trace complet,
            // dar clientul primeste un mesaj generic - stack trace-ul in
            // productie ar expune structura interna a aplicatiei.
            _logger.LogError(ex, "Eroare neasteptata pe {Path}", context.Request.Path);

            var detail = _env.IsDevelopment()
                ? ex.ToString()
                : "A aparut o eroare interna. Incercati din nou mai tarziu.";

            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, detail);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        // Daca raspunsul a inceput deja sa fie trimis, headerele nu mai pot fi schimbate.
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        // ProblemDetails = RFC 7807, formatul standard de eroare pentru API-uri.
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                404 => "Resursa nu a fost gasita",
                400 => "Cerere invalida",
                403 => "Acces interzis",
                _   => "Eroare de server"
            },
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    // Extension method (Lab 1) care ascunde UseMiddleware<T> in spatele
    // unui nume citibil: app.UseGlobalExceptionHandling();
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}

