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
using OfflineWebsiteViewer.Browser.Handlers;
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
            string[] args;
            try
            {
                args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                if (args == null)
                    args = e.Args;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error loading ActivationAcrgumens, fallback to e.Args");
                args = e.Args;
            }

            try
            {
                Start(args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to start: {ex.Message}");
            }
            
        }

        private void Start(string[] args)
        {
            Logger.Trace($"App startup with {args.Length} args: {String.Join(",", args)}");
            Logger.Trace("Configuring CefSettings");
            var cefSettings = new CefSettings();

            cefSettings.CefCommandLineArgs.Add("disable-gpu", "1");

            //cefSettings.SetOffScreenRenderingBestPerformanceArgs();
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
            if (args.Any())
            {
                var projectPath = args.First();
                Logger.Trace($"Startup project path: '{projectPath}'");

                if (projectPath.StartsWith("file://"))
                    projectPath = new Uri(projectPath).LocalPath;

                Logger.Trace($"Getting startup project: '{projectPath}'");
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
        }
    }
}
