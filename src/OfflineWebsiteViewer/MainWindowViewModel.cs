using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using NLog;
using OfflineWebsiteViewer.Annotations;
using OfflineWebsiteViewer.Browser;
using OfflineWebsiteViewer.Logging.Views;
using OfflineWebsiteViewer.Project;
using OfflineWebsiteViewer.Search;
using OfflineWebsiteViewer.Utility;
using Ookii.Dialogs.Wpf;

namespace OfflineWebsiteViewer
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public event PropertyChangedEventHandler PropertyChanged;

        private string _searchQuery;
        public string SearchField
        {
            get { return _searchQuery; }
            set
            {
                _searchQuery = value;
                OnSearchFieldChanged(_searchQuery);
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        
        public IProject Project
        {
            get { return (Application.Current as App)?.Project; }
            set
            {
                var app = Application.Current as App;
                if (app != null)
                {
                    app.Project = value;
                    OnPropertyChanged(nameof(Project));
                }
            }
        }

        public static bool IsDebug
        {
#if DEBUG
            get { return true; }
#else
            get { return false; }
#endif
        }

        private ObservableCollection<HtmlFileRecord> _searchResults;
        public ObservableCollection<HtmlFileRecord> SearchResults
        {
            get { return _searchResults; }
            set
            {
                _searchResults = value;
                OnPropertyChanged(nameof(SearchResults));
            }
        }

        public ICommand SearchResultsSelectionChangedCommand { get; }
        public GenericCommand GoHomeCommand { get; }
        public ICommand OpenDevToolsCommand { get; }
        public ICommand OpenArchiveCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand ClearRecent { get; }
        public ICommand OpenLog { get; }
        public GenericCommand CreateIndexCommand { get; }
        public GenericCommand ClearIndexCommand { get; }
        public List<IProject> Recent { get; }
        public IProject RemovableProject { get; private set; }

        private readonly ChromiumWebBrowser _browser;
        private readonly BrowserBinding _binding;
        private readonly RegistryRecentProjectsPersister _persister = new RegistryRecentProjectsPersister(5);

        public MainWindowViewModel(ChromiumWebBrowser browser)
        {
            Logger.Trace("Main window started");
            _browser = browser;
            _browser.DownloadHandler = new CustomDownloadHandler();
            _browser.LifeSpanHandler = new CustomLifespanHandler();

            OpenDevToolsCommand = new GenericCommand(() =>
            {
#if DEBUG
                _browser.ShowDevTools();
#else
                return;
#endif
            });

            OpenLog = new GenericCommand(() => { LoggingView.Instance.Show(); });

            GoHomeCommand = new GenericCommand(NavigateTohome, () => Project != null);
            SearchResultsSelectionChangedCommand = new GenericCommand<HtmlFileRecord>(NavigateTo);
            ClearRecent = new GenericCommand(() =>
            {
                if(_persister.RecentProjects.Any())
                    _persister?.Clear();
            });
            OpenArchiveCommand = new GenericCommand(() =>
            {
                var extFilter = String.Join("; ", ArchiveProject.Extensions.Select(e => "*" + e)); ;
                var dialog = new OpenFileDialog
                {
                    
                    Filter =
                        $"Projects ({extFilter})|{extFilter}|All files (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    Open(dialog.FileName);
                }
            });
            OpenFolderCommand = new GenericCommand(() =>
            {
                var dialog = new VistaFolderBrowserDialog();
                if (dialog.ShowDialog() == true)
                {
                    Open(dialog.SelectedPath);
                }
            });

            OpenCommand = new GenericCommand<string>(Open);

            CreateIndexCommand = new GenericCommand(
                () =>
                {
                    Status = Resources.Language.StatusIndexCreating;
                    Project.CreateIndex(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Status = Resources.Language.StatusIndexFinished;
                            CreateIndexCommand.RaiseCanExecuteChanged();
                            ClearIndexCommand.RaiseCanExecuteChanged();
                        });
                    });
                }, 
                () => Project?.SearchIndex?.IsEmptyIndex ?? false);

            ClearIndexCommand = new GenericCommand(
                () =>
                {
                    Project.SearchIndex.ClearIndex();
                    CreateIndexCommand.RaiseCanExecuteChanged();
                    ClearIndexCommand.RaiseCanExecuteChanged();
                }, 
                () => !Project?.SearchIndex?.IsEmptyIndex ?? false);

            Recent = _persister.RecentProjects.Select(GetProject).Where(p => p != null).ToList();
            Logger.Trace($"Loaded {Recent.Count} recent projects");

            FindProjectOnRemovableDrive();

            TemplateRenderer.RegisterSafeTypeWithAllProperties(typeof(IProject));
            browser.LoadHtml(TemplateRenderer.RenderFromResource("welcome.html", new {
                recent = Recent,
                removable = RemovableProject
            }), "resource://local/welcome");
            browser.Address = "resource://local/welcome";

            _binding = new BrowserBinding();
            _binding.Bind("open", new GenericCommand<string>(Open));
            _binding.Bind("open_removable", new GenericCommand(() => Open(RemovableProject)));
            _binding.Bind("open_archive", OpenArchiveCommand);
            _binding.Bind("open_folder", OpenFolderCommand);
            _binding.Bind("open_recent", new GenericCommand<int>(OpenRecent));
            _binding.Bind("search", (query) => Project.SearchIndex.Search(query as string));
            browser.RegisterJsObject("app", _binding);

            
            if (Project != null)
                LoadProject();
        }

        private void OnSearchFieldChanged(string query)
        {
            if(Project == null)
                return;

            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                SearchResults?.Clear();
                return;
            }
            Status = String.Format(Resources.Language.StatusSearching, query);
            Project.SearchIndex.Search(query, (results) =>
            {
                Status = String.Format(Resources.Language.StatusIndexFound, results.Count(), query);
                SearchResults = new ObservableCollection<HtmlFileRecord>(results);
            });
        }

        public void Open(IProject project)
        {
            if (project == null)
                return;

            Logger.Trace($"Opened project {project.Name}");
            Project = project;
            LoadProject();
        }

        private void LoadProject()
        {
            Status = String.Format(Resources.Language.StatusOpened, Project.Name);
            Project.Open();

            if (!Project.SearchIndex.IsEmptyIndex)
            {
                Status = String.Format(Resources.Language.StatusIndexCount, Project.SearchIndex.Count);
            }

            CreateIndexCommand.RaiseCanExecuteChanged();
            ClearIndexCommand.RaiseCanExecuteChanged();
            GoHomeCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Project));
            NavigateTohome();
            _persister.AddProject(Project.ProjectPath);
            OnPropertyChanged(nameof(Recent));
        }

        public void Open(string fileOrFolder)
        {
            var project = GetProject(fileOrFolder);
            if (project == null)
            {
                MessageBox.Show(String.Format(Resources.Language.ErrorCantOpenProject, fileOrFolder),
                    Resources.Language.Error,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Open(project);
            }
        }

        public void OpenRecent(int id)
        {
            if (id <= Recent.Count - 1)
            {
                Open(Recent[id]);
            }
        }

        public void NavigateTo(HtmlFileRecord record)
        {
            Logger.Trace($"Navigating to record '{record.Path}'");
            if (Project != null)
                _browser.Address = Project.GetUrl(record.Path);
        }

        public void NavigateTohome()
        {
            Logger.Trace($"Going home");
            if(Project != null)
                _browser.Address = Project.IndexUrl;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static IProject GetProject(string projectPath)
        {
            Logger.Trace($"Trying to resolve project by path '{projectPath}'");
            if (Directory.Exists(projectPath))
            {
                Logger.Trace($"Project located by path '{projectPath}' is a directory project");
                return new FlatDirectoryProject(projectPath);
            }

            if (File.Exists(projectPath))
            {
                var extension = Path.GetExtension(projectPath);
                if (extension != null && ArchiveProject.Extensions.Contains(extension))
                {
                    Logger.Trace($"Project located by path '{projectPath}' is an archive project");
                    return new ArchiveProject(projectPath);
                }
                else
                {
                    Logger.Trace($"Invalid extension");
                }
            }
           
            return null;
        }

        private void FindProjectOnRemovableDrive()
        {
            var driveList = DriveInfo.GetDrives();
            if(driveList.Any())
                Logger.Trace($"Found {driveList.Length} drives");

            foreach (var drive in driveList)
            {
                if ((drive.DriveType == DriveType.Removable || drive.DriveType == DriveType.CDRom) && drive.IsReady)
                {
                    Logger.Trace($"Checking {drive} for archives");
                    var root = drive.RootDirectory;
                    try
                    {
                        var archives = root.GetFiles("*.owr", SearchOption.TopDirectoryOnly);
                        if (archives.Any())
                        {
                            Logger.Trace($"Found {archives.Length} archives");
                            Logger.Trace($"Using {archives.First()} archive");
                            var project = new ArchiveProject(archives.First().FullName);
                            RemovableProject = project;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error reading removable drive");
                        throw;
                    }
                }
            }
        }
    }
}
