using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreCHILIDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreCHILIDemo.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<IdentityUser> userManger;
        private SignInManager<IdentityUser> signInManager;
        private readonly ChiliConnector chiliConnector;

        public HomeController(ChiliConnector chiliConnector)
        {
            this.chiliConnector = chiliConnector;
        }

        public async Task<ActionResult> Index(bool showMessage = false)
        {
            if (Startup.testMode)
            {
                string apiKey = await chiliConnector.GenerateApiKey("ChiliDemo", "ProfileUser", "password");
                HttpContext.Session.SetString("apiKey", apiKey);
                apiKey = await chiliConnector.GenerateApiKey("ChiliDemo", "ProfileServer", "password");
                HttpContext.Session.SetString("serverKey", apiKey);
                HttpContext.Session.SetString("environment", "ChiliDemo");
                HttpContext.Session.SetString("username", "Deadpool");
                return Redirect("/storefront/index");

            }

            ViewBag.showMessage = showMessage;

            string message = HttpContext.Session.GetString("loginMessage");

            if (message != null)
            {
                ViewBag.message = message;
            }

            return View();
        }

        // ChiliUser Chili#2020
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            HttpContext.Session.SetString("username", username);

            return Redirect("/home/UserAccount");
        }

        //[Authorize]
        public async Task<IActionResult> UserAccount()
        {
            if (HttpContext.User.Identity.IsAuthenticated == false)
            {
                HttpContext.Session.SetString("loginMessage", "Please Login");
                return Redirect("/home/index?showMessage=true");
            }

            // Get API Key
            string apiKey = await chiliConnector.GenerateApiKey("ChiliDemo", "ProfileUser", "password");
            string serverKey = await chiliConnector.GenerateApiKey("ChiliDemo", "ProfileServer", "password");

            if (apiKey != null || serverKey != null)
            {
                if (apiKey.ToLower().Contains("error"))
                {
                    ViewBag.error = true;
                    ViewBag.message = apiKey;
                }
                else
                {
                    HttpContext.Session.SetString("apiKey", apiKey);
                    HttpContext.Session.SetString("environment", "ChiliDemo");
                    HttpContext.Session.SetString("serverKey", serverKey);
                    
                }
            }

            return View();
        }
    }
}