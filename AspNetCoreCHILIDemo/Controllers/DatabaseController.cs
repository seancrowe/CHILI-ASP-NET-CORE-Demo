using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AspNetCoreCHILIDemo.Controllers
{
    public class DatabaseController : Controller
    {
        [HttpPost]
        public ActionResult save()
        {
            Stream req = Request.Body;
            StreamReader streamReader = new StreamReader(req);
            string json = streamReader.ReadToEnd();

            if (json != null)
            {
                var objectJson = JsonConvert.DeserializeObject(json);

                // Save in database
            }

            return Json(new { succes = "true" });
        }

    }
}