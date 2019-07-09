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
    public class StoreFrontController : Controller
    {
        private readonly ChiliConnector chiliConnector;

        public StoreFrontController(ChiliConnector chiliConnector)
        {
            this.chiliConnector = chiliConnector;
        }

        // GET: StoreFront
        public async Task<ActionResult> Index()
        {
            //string apiKey = HttpContext.Session.GetString("apiKey");
            string apiKey = HttpContext.Session.GetString("serverKey");

            ViewBag.templates = null;

            if (apiKey != null)
            {
                // Get a list of all templates we can use
                IEnumerable<ChiliResource> templateList = await chiliConnector.GetResourceInFolder(apiKey, "Templates/Active/", ChiliResource.ResourceType.Documents, 1);

                // Serialize to JSON
                string templateJsonArray = JsonConvert.SerializeObject(templateList.ToArray());
                //string templateJsonArray = Json(templateList.ToArray()).ToString();
                
                // Store in Session
                // TODO: Switch to Cache
                HttpContext.Session.SetString("currentChoicesArray", templateJsonArray);


                ViewBag.templates = templateList;
                ViewBag.templateCount = templateList.Count();
            }

            return View();
        }

        public async Task<ActionResult> Edit(string id)
        {
            //string apiKey = HttpContext.Session.GetString("apiKey");
            string apiKey = HttpContext.Session.GetString("serverKey");

            if (apiKey != null)
            {
                string templatesJsonArray = HttpContext.Session.GetString("currentChoicesArray");

                if (templatesJsonArray != null)
                {
                    // Deserialize from session
                    ChiliResource[] templatesArray = JsonConvert.DeserializeObject<ChiliResource[]>(templatesJsonArray);
                    ChiliResource template = templatesArray.Single(cr => cr.id == id);

                    string userName = HttpContext.Session.GetString("username");

                    string date = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.DayOfWeek.ToString();

                    ChiliResource newTemplate = await chiliConnector.ResourceItemCopy(apiKey, ChiliResource.ResourceType.Documents, template.id, $"Users/{userName}/temp/{date}");

                    if (newTemplate != null)
                    {
                        HttpContext.Session.SetString("currentWorkingTemplate", JsonConvert.SerializeObject(newTemplate));

                        return Redirect("/storefront/editor");
                    }
                }
            }

            return Redirect("/storefront/index");
        }

        public ActionResult Editor()
        {
            string apiKey = HttpContext.Session.GetString("apiKey");

            if (apiKey != null)
            {

                string newTemplateJson = HttpContext.Session.GetString("currentWorkingTemplate");

                if (newTemplateJson != null)
                {
                    ChiliResource newTemplate = JsonConvert.DeserializeObject<ChiliResource>(newTemplateJson);

                    string environment = HttpContext.Session.GetString("environment");

                    ViewBag.templateCopyId = newTemplate.id;
                    ViewBag.editorUrl = chiliConnector.GetEditorUrl(apiKey, newTemplate.id, environment);

                    return View();
                }
            }

           return Redirect("/storefront/index");
        }

        public ActionResult Order()
        {
            return View();
        }
    }
}