using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreCHILIDemo.Objects;
using AspNetCoreCHILIDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AspNetCoreCHILIDemo.Controllers
{
    public class OrdersController : Controller
    {
        private ChiliConnector chiliConnector;

        public OrdersController(ChiliConnector chiliConnector)
        {
            this.chiliConnector = chiliConnector;
        }

        // GET: Orders
        public ActionResult Index()
        {
            return Redirect("/storefront/index");
        }

        [HttpPost]
        public async Task<ActionResult> NewOrder()
        {
            //string apiKey = HttpContext.Session.GetString("apiKey");

            string environment = HttpContext.Session.GetString("environment");
            string apiKey = await chiliConnector.GenerateApiKey(environment, "ProfileServer", "password");

            if (apiKey != null)
            {

                string currentTemplateJson = HttpContext.Session.GetString("currentWorkingTemplate");
                if (currentTemplateJson != null)
                {
                    ChiliResource currentTemplate = JsonConvert.DeserializeObject<ChiliResource>(currentTemplateJson);

                    Task<string> pdfUrlTask = chiliConnector.DocumentCreatePdf(apiKey, currentTemplate.id, "76e02f73-c687-4ae3-b5e7-f6e5f2c707e8");

                    while (pdfUrlTask.IsCompleted == false)
                    {

                    }

                    string pdfUrl = pdfUrlTask.Result;

                    if (!pdfUrl.ToLower().Contains("error"))
                    {
                        return Json(new { success = "true", url = pdfUrl });
                    }
                }

            }



            return Json(new { success = "false" });
        }
        
    }
}