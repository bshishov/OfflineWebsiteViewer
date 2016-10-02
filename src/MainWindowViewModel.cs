using System;
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
using Newtonsoft.Json;
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
        public GenericCommand CreateIndexCommand { get; }
        public GenericCommand ClearIndexCommand { get; }

        private readonly ChromiumWebBrowser _browser;

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

            CreateIndexCommand = new GenericCommand(
                () =>
                {
                    Project.CreateIndex(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
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
        }

        private void OnSearchFieldChanged(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                SearchResults?.Clear();
                return;
            }

            Project.SearchIndex.Search(query, (results) =>
            {
                Status = $"Found {results.Count()} results matching query {query}";
                SearchResults = new ObservableCollection<HtmlFileRecord>(results);
            });
        }

        public void Open(IProject project)
        {
            Project = project;
            Status = $"Project {Project} loaded";

            if (!Project.SearchIndex.IsEmptyIndex)
            {
                Status = $"Found {Project.SearchIndex.Count} pages in index";
            }

            CreateIndexCommand.RaiseCanExecuteChanged();
            ClearIndexCommand.RaiseCanExecuteChanged();
            GoHomeCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Project));
        }

        public void OpenDirectory(string directoryPath)
        {
            Open(new FlatDirectoryProject(directoryPath));
        }

        public void OpenArchive(string archivePath)
        {
            Open(new ArchiveProject(archivePath));
        }


        public void NavigateTo(HtmlFileRecord record)
        {
            _browser.Address = Project.GetUrl(record.Path);
        }

        public void NavigateTohome()
        {
            _browser.Address = Project?.GetUrl(Project.IndexFile);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
