using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreCHILIDemo.Objects;
using AspNetCoreCHILIDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AspNetCoreCHILIDemo.Controllers
{
    public class StoreManagerController : Controller
    {
        private ChiliConnector _chiliConnector;

        public StoreManagerController(ChiliConnector chiliConnector)
        {
            _chiliConnector = chiliConnector;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IList<IFormFile> files)
        {
            string apiKey = HttpContext.Session.GetString("apiKey");
            string serverKey = HttpContext.Session.GetString("serverKey");


            if (apiKey != null && serverKey != null)
            {
                List<CopyFileTask> copyFileTasks = new List<CopyFileTask>();

                foreach (IFormFile file in files)
                {
                    if (file.FileName.Contains("zip"))
                    {
                        string fileName = file.FileName.Replace(".zip", "");

                        if (file.Length > 0)
                        {
                            Random random = new Random();
                            string path = @"D:\Data\temp\" + random.Next(1000);
                            Directory.CreateDirectory(path);

                            string filePath = path + "\\" +  file.FileName;

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                Task task = file.CopyToAsync(fileStream);

                                copyFileTasks.Add(new CopyFileTask(task, fileName, filePath));
                            }
                        }


                        
                    }
                }

                while (copyFileTasks.Count > 0)
                {
                    for (int i = copyFileTasks.Count - 1; i >= 0; i--)
                    {
                        CopyFileTask copyFileTask = copyFileTasks[i];

                        if (copyFileTask.task.IsCompleted)
                        {
                            copyFileTasks.RemoveAt(i);

                            if (copyFileTask.task.IsCompletedSuccessfully)
                            {
                                string fileName = copyFileTask.fileName;
                                string filePath = copyFileTask.filePath;
                                int taskId = copyFileTask.task.Id;
                                string boPath = "From Illustrator/" + taskId;

                                Task<ChiliService.DocumentCreateFromChiliPackageResponse> createFromPdfResponse = _chiliConnector.soapClient.DocumentCreateFromChiliPackageAsync(serverKey, fileName, boPath, filePath, boPath, boPath);
                            }
                        }

                    }
                }
            }

            return Json(new { complete = "true" });
        }
    }
}
