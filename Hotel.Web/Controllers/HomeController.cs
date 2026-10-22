using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Hotel.Web.Models;
using Hotel.Web.Repositorios;

namespace Hotel.Web.Controllers;

public class HomeController : Controller
{
    private readonly HabitacionRepositorio _habitaciones;

    public HomeController(HabitacionRepositorio habitaciones)
    {
        _habitaciones = habitaciones;
    }

    // GET: / (página de inicio con los tipos de habitación destacados)
    public async Task<IActionResult> Index()
    {
        return View(await _habitaciones.ListarTiposConHabitacionesAsync());
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
