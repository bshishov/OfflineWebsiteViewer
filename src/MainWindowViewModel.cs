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
using OfflineWebsiteViewer.Commands;
using OfflineWebsiteViewer.Search;

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
                Console.WriteLine($"CHANGED to {value}");
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

        private OfflineWebResourceProject _project;
        public OfflineWebResourceProject Project
        {
            get { return _project; }
            set
            {
                _project = value;
                OnPropertyChanged(nameof(Project));
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

        public ICommand SearchResultsSelectionChangedCommand { get; private set; }
        public ICommand GoHomeCommand { get; private set; }
        public ICommand OpenDevTools { get; private set; }
        public ICommand OpenCommand { get; private set; }

        private readonly ChromiumWebBrowser _browser;
        private HtmlFileSearch _search;

        public MainWindowViewModel(ChromiumWebBrowser browser)
        {
            _browser = browser;
            _browser.DownloadHandler = new CustomDownloadHandler();

            OpenDevTools = new GenericCommand(() =>
            {
                _browser.ShowDevTools();
            });

            GoHomeCommand = new GenericCommand(NavigateTohome);
            SearchResultsSelectionChangedCommand = new GenericCommand<HtmlFileRecord>(NavigateTo);
            OpenCommand = new GenericCommand(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = $"Projects (*{OfflineWebResourceProject.Extenstion})|*{OfflineWebResourceProject.Extenstion}|All files (*.*)|*.*";
                if (dialog.ShowDialog() == true)
                {
                    Open(dialog.FileName);
                }
            });
        }

        private void OnSearchFieldChanged(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                SearchResults?.Clear();
                return;
            }

            _search.Search(query, (results) =>
            {
                Status = $"Found {results.Count()} results matching query {query}";
                SearchResults = new ObservableCollection<HtmlFileRecord>(results);
            });
        }

        public void Open(OfflineWebResourceProject project)
        {
            Project = project;
            Status = $"Project {Project.Name} loaded";

            _search = new HtmlFileSearch(Project);

            if (Project.IndexSearch && _search.IsEmptyIndex)
            {
                Status = $"Indexing";
                _search.RunReindexTask();
            }
            else
            {
                Status = $"Found {_search.Count} pages in index";
            }
        }

        public void Open(string projectPath)
        {
            var project = JsonConvert.DeserializeObject<OfflineWebResourceProject>(File.ReadAllText(projectPath), 
                new JsonSerializerSettings(){ });
            project.ProjectPath = new FileInfo(projectPath).DirectoryName;

            Open(project);
        }

        public void NavigateTo(HtmlFileRecord record)
        {
            _browser.Address = Path.Combine(_project.ProjectPath, record.Path);
        }

        public void NavigateTohome()
        {
            _browser.Address = _project.IndexFilePath;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
