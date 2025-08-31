using Microsoft.AspNetCore.Mvc;

namespace FirstAppMvc.Controllers
{
    public class ArticlesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
