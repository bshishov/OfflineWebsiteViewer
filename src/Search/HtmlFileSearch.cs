using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OfflineWebsiteViewer.Search
{
    class HtmlFileSearch
    {
        public bool IsEmptyIndex => !_index.GetAllIndexRecords().Any();
        public int Count => _index.GetAllIndexRecords().Count();

        private readonly OfflineWebResourceProject _project;
        private readonly LuceneIndex<HtmlFileRecord> _index;
        private readonly Regex _titleRegex;
        private CancellationTokenSource _searchCts;
        private Task _currentSearchTask;

        public HtmlFileSearch(OfflineWebResourceProject project)
        {
            _project = project;
            _titleRegex = new Regex(@"<title>(.*?)</title>", RegexOptions.Compiled);
            _index = new LuceneIndex<HtmlFileRecord>(Path.Combine(_project.ProjectPath, "SearchIndex"), new HtmlFileMapper());
            var all = _index.GetAllIndexRecords();
        }

        private void ReIndex()
        {
            var files = System.IO.Directory.EnumerateFiles(_project.ProjectPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".htm", StringComparison.OrdinalIgnoreCase));

            var records = new List<HtmlFileRecord>();
            foreach (var file in files)
            {
                var relativePath = GetRelativePath(file, _project.ProjectPath);
                var text = File.ReadAllText(Path.Combine(_project.ProjectPath, relativePath));

                var result = _titleRegex.Match(text);
                var title = result.Groups[1].Value;
                Console.WriteLine($"Adding to index {relativePath}: {title}");

                records.Add(new HtmlFileRecord()
                {
                    Path = relativePath,
                    Title = title
                });
            }

            _index.AddUpdateLuceneIndex(records);
        }

        public void RunReindexTask()
        {
            Task.Factory.StartNew(ReIndex);
        }

        private string GetRelativePath(string filespec, string folder)
        {
            if (!folder.EndsWith("\\") || !folder.EndsWith("/"))
                folder += Path.DirectorySeparatorChar;

            var pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public void Search(string query, Action<List<HtmlFileRecord>> onComplete)
        {
            if(onComplete == null)
                return;

            // cancel previous search task 
            if (_currentSearchTask != null && !_currentSearchTask.IsCompleted)
            {
                _searchCts?.Cancel();
            }

            _searchCts = new CancellationTokenSource();

            _currentSearchTask = Task.Factory.StartNew(() =>
            {
                var results = _index.Search(query).ToList();
                onComplete(results);
            }, _searchCts.Token);
        }
    }
}
