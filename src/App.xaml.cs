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
using OfflineWebsiteViewer.Handlers;
using OfflineWebsiteViewer.Project;

namespace OfflineWebsiteViewer
{
    partial class App : Application
    {
        public IProject Project { get; set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var cefSettings = new CefSettings();
#if DEBUG
            cefSettings.CefCommandLineArgs.Add("disable-gpu", "1");
#else
            cefSettings.SetOffScreenRenderingBestPerformanceArgs();
#endif

            //cefSettings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            //cefSettings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");


            cefSettings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = ZipSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new ZipSchemeHandlerFactory(),
            });

            cefSettings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = ResourceSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new ResourceSchemeHandlerFactory("OfflineWebsiteViewer.Resources.Web"),
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
