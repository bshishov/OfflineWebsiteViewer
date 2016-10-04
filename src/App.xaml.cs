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
using NLog;
using NLog.Config;
using OfflineWebsiteViewer.Handlers;
using OfflineWebsiteViewer.Project;

namespace OfflineWebsiteViewer
{
    partial class App : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IProject _project;
        public IProject Project
        {
            get { return _project; }
            set
            {
                _project?.Dispose();
                _project = value;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Trace($"App startup with {e.Args.Length} args");
            Logger.Trace("Configuring CefSettings");
            var cefSettings = new CefSettings();
#if DEBUG
            cefSettings.CefCommandLineArgs.Add("disable-gpu", "1");
#else
            cefSettings.SetOffScreenRenderingBestPerformanceArgs();
#endif

            //cefSettings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            //cefSettings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");

            Logger.Trace($"Registing scheme {ZipSchemeHandlerFactory.SchemeName}");
            cefSettings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = ZipSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new ZipSchemeHandlerFactory(),
            });

            Logger.Trace($"Registing scheme {ResourceSchemeHandlerFactory.SchemeName}");
            cefSettings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = ResourceSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new ResourceSchemeHandlerFactory("OfflineWebsiteViewer.Resources.Web"),
            });

            Cef.Initialize(cefSettings);

            // if project is passed to arguments, open it
            if (e.Args.Length > 0)
            {
                var projectPath = e.Args[0];
                Logger.Trace($"Getting project from startup arg: '{e.Args[0]}'");
                Project = MainWindowViewModel.GetProject(projectPath);
                if (Project != null)
                {
                    Logger.Trace($"Project {Project.Name} resolved");
                }
                else
                {
                    Logger.Trace($"Passed project can't be resolved");
                }
            }

            //ConfigurationItemFactory.Default.Targets.RegisterDefinition("ShoutingTarget", typeof(Logging.ShoutingTarget));
        }
    }
}
