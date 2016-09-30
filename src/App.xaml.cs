using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using Newtonsoft.Json;

namespace OfflineWebsiteViewer
{
    public partial class App : Application
    {
        public OfflineWebResourceProject Project { get; private set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var cefSettings = new CefSettings();
            cefSettings.SetOffScreenRenderingBestPerformanceArgs();
            cefSettings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = "zip",
                SchemeHandlerFactory = new ZipSchemeHandlerFactory()
            });
            Cef.Initialize(cefSettings);

            if (e.Args.Length == 0)
            {
                MessageBox.Show("Project not selected", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var filename = e.Args[0];
                Project = JsonConvert.DeserializeObject<OfflineWebResourceProject>(File.ReadAllText(filename), new JsonSerializerSettings()
                {
                });

                Project.ProjectPath = new FileInfo(filename).DirectoryName;
            }
        }
    }
}
