using Microsoft.AspNetCore.Mvc;

namespace Movies.Controllers
{
    public class MoviesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
