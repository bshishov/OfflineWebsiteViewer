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
using OfflineWebsiteViewer.Project;

namespace OfflineWebsiteViewer
{
    partial class App : Application
    {
        public IProject Project { get; set; }
        
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
                var projectPath= e.Args[0];
                if (Directory.Exists(projectPath))
                {
                    Project = new FlatDirectoryProject(projectPath);
                }
                else
                {
                    var extension = Path.GetExtension(projectPath);
                    if(extension != null && extension.Equals(ArchiveProject.Extension))
                    {
                        Project = new ArchiveProject(projectPath);
                    }
                }
            }
        }
    }
}
