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
using OfflineWebsiteViewer.Annotations;
using OfflineWebsiteViewer.Project;
using OfflineWebsiteViewer.Search;
using OfflineWebsiteViewer.Utility;
using Ookii.Dialogs.Wpf;

namespace OfflineWebsiteViewer
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
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
        public GenericCommand CreateIndexCommand { get; }
        public GenericCommand ClearIndexCommand { get; }
        public List<IProject> Recent { get; }

        private readonly ChromiumWebBrowser _browser;
        private readonly BrowserBinding _binding;
        private readonly RegistryRecentProjectsPersister _persister = new RegistryRecentProjectsPersister(5);

        public MainWindowViewModel(ChromiumWebBrowser browser)
        {
            _browser = browser;
            _browser.DownloadHandler = new CustomDownloadHandler();

            OpenDevToolsCommand = new GenericCommand(() =>
            {
                _browser.ShowDevTools();
            });

            GoHomeCommand = new GenericCommand(NavigateTohome, () => Project != null);
            SearchResultsSelectionChangedCommand = new GenericCommand<HtmlFileRecord>(NavigateTo);
            OpenArchiveCommand = new GenericCommand(() =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter =
                        $"Projects (*{ArchiveProject.Extension})|*{ArchiveProject.Extension}|All files (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    OpenArchive(dialog.FileName);
                }
            });
            OpenFolderCommand = new GenericCommand(() =>
            {
                var dialog = new VistaFolderBrowserDialog();
                if (dialog.ShowDialog() == true)
                {
                    OpenDirectory(dialog.SelectedPath);
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

            Recent = _persister.RecentProjects.Select(GetProject).ToList();
            TemplateRenderer.RegisterSafeTypeWithAllProperties(typeof(IProject));
            browser.LoadHtml(TemplateRenderer.RenderFromResource("welcome.html", new {
                recent = Recent,
            }), "resource://local/welcome");
            browser.Address = "resource://local/welcome";

            _binding = new BrowserBinding();
            _binding.Bind("open_archive", OpenArchiveCommand);
            _binding.Bind("open_folder", OpenFolderCommand);
            _binding.Bind("open_recent", new GenericCommand<int>(OpenRecent));
            browser.RegisterJsObject("app", _binding);
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
            Project = project;
            Status = String.Format(Resources.Language.StatusOpened, Project.Name);

            if (!Project.SearchIndex.IsEmptyIndex)
            {
                Status = String.Format(Resources.Language.StatusIndexCount, Project.SearchIndex.Count);
            }

            CreateIndexCommand.RaiseCanExecuteChanged();
            ClearIndexCommand.RaiseCanExecuteChanged();
            GoHomeCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Project));
            NavigateTohome();
        }

        public void OpenDirectory(string directoryPath)
        {
            _persister.AddProject(directoryPath);
            Open(new FlatDirectoryProject(directoryPath));
        }

        public void OpenArchive(string archivePath)
        {
            _persister.AddProject(archivePath);
            Open(new ArchiveProject(archivePath));
        }

        public void Open(string fileOrFolder)
        {
            Open(GetProject(fileOrFolder));
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
            if (Project != null)
                _browser.Address = Project.GetUrl(record.Path);
        }

        public void NavigateTohome()
        {
            if(Project != null)
                _browser.Address = Project.IndexUrl;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private IProject GetProject(string fileOrFolder)
        {
            if (Directory.Exists(fileOrFolder))
                return new FlatDirectoryProject(fileOrFolder);
            else if (File.Exists(fileOrFolder))
                return new ArchiveProject(fileOrFolder);

            return null;
        }
    }
}
