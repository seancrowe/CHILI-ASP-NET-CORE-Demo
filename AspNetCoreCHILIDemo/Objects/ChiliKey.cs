using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace AspNetCoreCHILIDemo.Objects
{
    public class ChiliKey
    {
        public DateTime expires;
        public string environment;
        public string language;
        public string userEnvironment;
        public string userID;
        public string username;
        public string key;
        public bool noAutomaticPreviewForNewItems;
        public string userAssetDirectory;
        public string userGroupAssetDirectory;
        public string documentAssetDirectory;

        public ChiliKey(ChiliService.ApiKeyGetCurrentSettingsResponseBody settingsResponseBody)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(settingsResponseBody.ApiKeyGetCurrentSettingsResult);

            XmlAttributeCollection attributes =  xmlDocument.FirstChild.Attributes;

            username = attributes["environmentName"].Value;
        }

    }
}
