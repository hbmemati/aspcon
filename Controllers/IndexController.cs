using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace rgb565_converter.Controllers
{
    public class IndexController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
           // HttpContext.Request.Query["page"].ToString();
            return View();
        }
    }
}
