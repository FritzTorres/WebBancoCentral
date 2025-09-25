using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BancoCentralWeb.Models;
using BancoCentralWeb.Filters;

namespace BancoCentralWeb.Controllers
{
    [TypeFilter(typeof(SessionAuthorizeAttribute))]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            var usuario = HttpContext.Session.GetString("Usuario");
            
            _logger.LogInformation("HomeController.Index - SessionId: {SessionId}, Usuario: {Usuario}", sessionId, usuario);
            
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
}