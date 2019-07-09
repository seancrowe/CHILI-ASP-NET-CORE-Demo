using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml;
using AspNetCoreCHILIDemo.Objects;
using ChiliService;

namespace AspNetCoreCHILIDemo.Services
{
    public class ChiliConnector
    {
        public mainSoapClient soapClient;

        public readonly string url;

        public ChiliConnector(string url)
        {
            this.url = url;

            if (ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            // Stop 417 exception
            ServicePointManager.Expect100Continue = false;

            EndpointAddress endpointAddress = new EndpointAddress(url);

            if (url.Contains("https"))
            {
                BasicHttpsBinding basicHttpsBinding = new BasicHttpsBinding();
                basicHttpsBinding.MaxReceivedMessageSize = int.MaxValue;
                basicHttpsBinding.OpenTimeout = new TimeSpan(0, 20, 0);
                basicHttpsBinding.CloseTimeout = new TimeSpan(0, 20, 0);
                basicHttpsBinding.SendTimeout = new TimeSpan(0, 20, 0);
                basicHttpsBinding.ReceiveTimeout = new TimeSpan(0, 20, 0);
                soapClient = new mainSoapClient(basicHttpsBinding, endpointAddress);
            }
            else
            {
                BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
                basicHttpBinding.MaxReceivedMessageSize = int.MaxValue;
                basicHttpBinding.OpenTimeout = new TimeSpan(0, 20, 0);
                basicHttpBinding.CloseTimeout = new TimeSpan(0, 20, 0);
                basicHttpBinding.SendTimeout = new TimeSpan(0, 20, 0);
                basicHttpBinding.ReceiveTimeout = new TimeSpan(0, 20, 0);
                soapClient = new mainSoapClient(basicHttpBinding, endpointAddress);
            }
        }

        public ChiliKey ApiKeyGetSettings(string apiKey)
        {
            Task<ApiKeyGetCurrentSettingsResponse> task = soapClient.ApiKeyGetCurrentSettingsAsync(apiKey);

            while(task.IsCompleted == false)
            {

            }

            return new ChiliKey(task.Result.Body);
        }

        public async Task<string> DocumentCreatePdf(string apiKey, string documentId, string pdfExportSettingsId, int taskPriority = 6)
        {
            ChiliTask task = DocumentCreatePdfTask(apiKey, documentId, pdfExportSettingsId, taskPriority = 6);

            bool taskComplete = false;

            while (taskComplete == false)
            {
                task = TaskGetStatus(apiKey, task.id);

                taskComplete = task.finished;

                if (task.found == false)
                {
                    taskComplete = true;  
                }
            }

            if (task.found == true)
            {
                if (task.succeeded == false)
                {
                    return "Error: " + task.errorMessage;
                }
                else
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(task.result);

                    string downloadUrl = xmlDocument.FirstChild.Attributes["url"].Value;

                    return downloadUrl;
                }
            }
            else
            {
                return "Error: Task was lost";
            }


            
        }

        public ChiliTask DocumentCreatePdfTask(string apiKey, string documentId, string pdfExportSettingsId, int taskPriority = 6)
        {
            string pdfSettings = ResourceItemGetXml(apiKey, "PdfExportSettings", pdfExportSettingsId);

            if (taskPriority < 2)
            {
                taskPriority = 2;
            }

            Task<DocumentCreatePDFResponse> createPDFResponse = soapClient.DocumentCreatePDFAsync(apiKey, documentId, pdfSettings, taskPriority);

            while (createPDFResponse.IsCompleted == false)
            {

            }

            string taskXmlString = createPDFResponse.Result.Body.DocumentCreatePDFResult;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(taskXmlString);

            XmlNodeList itemNodes = xmlDocument.GetElementsByTagName("task");

            if (itemNodes.Count > 0)
            {
                XmlNode itemNode = itemNodes[0];

                return new ChiliTask(itemNode);
            }

            return null;
        }

        public async Task<string> GenerateApiKey(string environment, string username, string password)
        {
            GenerateApiKeyResponse response = null;

            try
            {
                response = await soapClient.GenerateApiKeyAsync(environment, username, password);
            }
            catch (Exception e)
            {
                return "Error! Cannot connect to CHILI server";
            }

            if (response != null)
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(response.Body.GenerateApiKeyResult);

                if (xmlDocument.FirstChild.Attributes["succeeded"] != null && xmlDocument.FirstChild.Attributes["succeeded"].Value == "true")
                {
                    return xmlDocument.FirstChild.Attributes["key"].Value;
                }
                else
                {
                    return "Error! API key has wrong credentials";
                }
            }

            return null;
        }

        public string GetEditorUrl(string apiKey, string id, string environment = null, string workspace = null, string viewPreferences = null, string constraints = null, string[] queries = null)
        {
            if (environment == null)
            {
                environment = ApiKeyGetSettings(apiKey).environment;
            }

            string editorUrl = url.Replace("main.asmx", $"/{environment}/editor_html.aspx?apiKey={apiKey}&doc={id}");

            if (workspace != null)
            {
                editorUrl += $"&ws={workspace}";
            }

            if (viewPreferences != null)
            {
                editorUrl += $"&vp={viewPreferences}";
            }

            if (constraints != null)
            {
                editorUrl += $"&c={constraints}";
            }

            if (queries != null && queries.Count() > 0)
            {
                //TODO add parameters
            }

            return editorUrl;
        }

        public enum PreviewType
        {
            thumb,
            medium,
            full,
        }

        public string GetResourcePreviewUrl(string apiKey, ChiliResource chiliResource, PreviewType previewType)
        {
            return GetResourcePreviewUrl(apiKey, chiliResource.resourceType, chiliResource.id, previewType);
        }

        public string GetResourcePreviewUrl(string apiKey, ChiliResource.ResourceType resourceType, string id, PreviewType previewType)
        {
            return GetResourceDownloadUrl(apiKey, resourceType, id, previewType.ToString());
        }

        public string GetResourceDownloadUrl(string apiKey, ChiliResource.ResourceType resourceType, string id, string downloadType = "original", bool async = false, int taskPriority = 0, int? pageNumber = null)
        {
            string environmnet = ApiKeyGetSettings(apiKey).environment;

            string requestUrl = url.Replace("main.asmx", environmnet) + $"/download.aspx?apiKey={apiKey}&resourceName={resourceType.ToString()}&id={id}&type={downloadType}&async={async.ToString().ToLower()}";

            // TODO I need do something different as async will return task XML if the preview does not exist yet
            if (async == true)
            {
                requestUrl += $"&taskPriority={taskPriority}";
            }

            if (pageNumber != null)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }

                requestUrl += $"&pageNum={pageNumber}";
            }


            return requestUrl;

        }

        public async Task<IEnumerable<ChiliResource>> GetResourceInFolder(string apiKey, string parentFolder, ChiliResource.ResourceType resourceType = ChiliResource.ResourceType.Documents, int numLevels = 1)
        {
            List<ChiliResource> resourcesFound = new List<ChiliResource>();

            ResourceGetTreeLevelResponse treeResponse = await soapClient.ResourceGetTreeLevelAsync(apiKey, resourceType.ToString(), parentFolder, numLevels);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(treeResponse.Body.ResourceGetTreeLevelResult);

            XmlNodeList parent = xmlDocument.GetElementsByTagName("tree");

            if (parent.Count > 0 && parent[0].HasChildNodes)
            {
                GetXmlItemsInFolder(parent[0], ref resourcesFound, resourceType, 0);
            }


            return resourcesFound;
        }

        private void GetXmlItemsInFolder(XmlNode folderNode, ref List<ChiliResource> resourceList, ChiliResource.ResourceType resourceType, int level)
        {
            foreach (XmlNode node in folderNode.ChildNodes)
            {
                if (node.Attributes["isFolder"] != null)
                {
                    if (node.Attributes["isFolder"].Value == "false")
                    {

                        string id = node.Attributes["id"].Value;
                        string name = node.Attributes["name"].Value;
                        string previewUrl = url.Replace("main.asmx", "") + node.Attributes["iconURL"].Value;
                        string path = node.Attributes["path"].Value;

                        ChiliResource chiliResource = new ChiliResource(id, name, resourceType)
                        {
                            previewUrl = previewUrl,
                            pathOnServer = path
                        };

                        resourceList.Add(chiliResource);
                    }
                    else
                    {
                        GetXmlItemsInFolder(node, ref resourceList, resourceType, level + 1);
                    }

                }
                
            }
        }

        public async Task<ChiliResource> ResourceItemCopy(string apiKey, ChiliResource.ResourceType resourceType, string id, string copyToPath, string newName = null)
        {
            XmlDocument xmlDocument = new XmlDocument();

            if (newName == null)
            {
                ResourceItemGetDefinitionXMLResponse responseGet = await soapClient.ResourceItemGetDefinitionXMLAsync(apiKey, resourceType.ToString(), id);
                
                xmlDocument.LoadXml(responseGet.Body.ResourceItemGetDefinitionXMLResult);

                XmlNode xmlInfoNode = xmlDocument.SelectSingleNode("//item[@name and @id]");

                if (xmlInfoNode == null)
                {
                    throw new NotImplementedException("Info node is blank and you didn't do anything thing");
                }
                else
                {
                    newName = xmlInfoNode.Attributes["name"].Value;
                }

            }

            ResourceItemCopyResponse response = await soapClient.ResourceItemCopyAsync(apiKey, resourceType.ToString(), id, newName, copyToPath);
            xmlDocument.LoadXml(response.Body.ResourceItemCopyResult);

            //<item name="Cat.zip" id="4a769cd7-a702-496f-be20-f59c9dc4ba18" relativePath="Blank\Cat.zip.xml" documentVersion="5.4.1.1" lastAcceptedVersion="5.4.1.1"><fileInfo fileIndexed="2019-05-12T17:00:33" numPages="1" resolution="72" width="595.266" height="841.8762" fileSize="13.15 Kb"><metaData><item name="Num. Pages" value="1" /><item name="Width" value="210 mm" /><item name="Height" value="297 mm" /></metaData><boxes /></fileInfo></item>
            XmlNodeList nodes = xmlDocument.GetElementsByTagName("item");

            if (nodes.Count > 0)
            {
                XmlNode xmlInfoNode = nodes[0];

                string resourceName = xmlInfoNode.Attributes["name"].Value;
                string resourceID = xmlInfoNode.Attributes["id"].Value;
                string resourcePath = xmlInfoNode.Attributes["relativePath"].Value;

                ChiliResource chiliResource = new ChiliResource(resourceID, resourceName, resourceType)
                {
                    pathOnServer = resourcePath,
                    previewUrl = GetResourcePreviewUrl(apiKey, resourceType, resourceID, PreviewType.thumb)
                };

                return chiliResource;

            }

            return null;
        }

        public string ResourceItemGetXml(string apiKey, string resourceName, string id)
        {
            Task<ResourceItemGetXMLResponse> responseTask = soapClient.ResourceItemGetXMLAsync(apiKey, resourceName, id);

            while (responseTask.IsCompleted == false)
            {

            }

            return responseTask.Result.Body.ResourceItemGetXMLResult;
        }

        public ChiliTask TaskGetStatus(string apiKey, string taskId)
        {
            Task<TaskGetStatusResponse> getStatusResponse = soapClient.TaskGetStatusAsync(apiKey, taskId);

            while(getStatusResponse.IsCompleted == false)
            {

            }

            string response = getStatusResponse.Result.Body.TaskGetStatusResult;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(response);

            XmlNodeList itemNodes = xmlDocument.GetElementsByTagName("task");

            if (itemNodes.Count > 0)
            {
               return new ChiliTask(itemNodes[0]); 
            }

            return null;
        }
    }
}
