using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreCHILIDemo.Objects
{
    public class ChiliResource
    {
        public enum ResourceType
        {
            Documents,
            Assets,
            Fonts,
            Workspaces,
            ViewPreferences
        }

        private string _name;
        public string name { get { return _name; } }
        private string _id;
        public string id { get { return _id; } }

        private ResourceType _resourceType;
        public ResourceType resourceType { get { return _resourceType; } }

        public string previewUrl { get; set;}

        public string pathOnServer { get; set; }

        public ChiliResource(string id, string name, ResourceType resourceType)
        {
            _name = name;
            _id = id;
            _resourceType = resourceType;
        }


    }
}
