using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotNet6MVC.Models;

namespace DotNet6MVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/{culture:regex(en)}/home")]
    [HttpGet("/{culture:regex(fr)}/hom")]
    public IActionResult Index()
    {
        return View();
    }


    [HttpGet("/{culture:regex(en)}/privacy")]
    [HttpGet("/{culture:regex(fr)}/privee")]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("/{culture:regex(en)}/error")]
    [HttpGet("/{culture:regex(fr)}/erreur")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
