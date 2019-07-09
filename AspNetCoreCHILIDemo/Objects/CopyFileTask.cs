using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreCHILIDemo.Objects
{
    public struct CopyFileTask
    {
        public Task task;
        public string fileName;
        public string filePath;

        public CopyFileTask(Task task, string fileName, string filePath)
        {
            this.task = task;
            this.fileName = fileName;
            this.filePath = filePath;
        }

    }
}
