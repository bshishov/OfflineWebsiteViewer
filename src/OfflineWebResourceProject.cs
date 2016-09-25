using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineWebsiteViewer
{
    public class OfflineWebResourceProject
    {
        public string ProjectPath { get; set; }

        public string Name;
        public string Description;
        public string IndexFile;
        public bool IndexSearch;


        public string IndexFilePath => Path.Combine(ProjectPath, IndexFile);

        public OfflineWebResourceProject()
        {
        }
    }
}
