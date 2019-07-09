using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace AspNetCoreCHILIDemo.Objects
{
    public class ChiliTask
    {
        public string id;
        public bool succeeded;
        public bool finished;
        public bool found;
        public string result;
        public string errorMessage;

        public ChiliTask(XmlNode taskNode)
        {
            XmlAttributeCollection taskAttributes = taskNode.Attributes;

            if (taskAttributes["found"] == null || taskAttributes["found"].Value.ToLower() != "false")
            {
                found = true;

                id = taskAttributes["id"].Value;

                if (taskAttributes["finished"].Value.ToLower() == "true")
                {
                    finished = true;

                    if (taskAttributes["succeeded"].Value.ToLower() == "true")
                    {
                        succeeded = true;

                        result = taskAttributes["result"].Value;
                    }
                    else
                    {
                        errorMessage = taskAttributes["errorMessage"].Value;
                    }
                }
            }
        }
    }
}
