using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace OfflineWebsiteViewer.Search
{
    public class HtmlFileIndex
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool IsEmptyIndex => !_index.GetAllIndexRecords().Any();
        public int Count => _index.GetAllIndexRecords().Count();

        private readonly string _indexPath;
        private readonly LuceneIndex<HtmlFileRecord> _index;
        private readonly Regex _titleRegex;
        private CancellationTokenSource _searchCts;
        private Task _currentSearchTask;

        public HtmlFileIndex(string indexDirectory)
        {
            Logger.Info($"Opening search index from '{indexDirectory}'");
            _indexPath = indexDirectory;
            _titleRegex = new Regex(@"<title>(.*?)</title>", RegexOptions.Compiled);
            _index = new LuceneIndex<HtmlFileRecord>(indexDirectory, new HtmlFileMapper());
        }

        private void AddUpdateFilesToIndex(string directory, Action callback = null)
        {
            var files = System.IO.Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".htm", StringComparison.OrdinalIgnoreCase));

            var records = new List<HtmlFileRecord>();
            foreach (var file in files)
            {
                var relativePath = GetRelativePath(file, directory);
                var text = File.ReadAllText(Path.Combine(directory, relativePath));

                var result = _titleRegex.Match(text);
                var title = result.Groups[1].Value;
                Logger.Info($"Adding to index {relativePath}: {title}");

                records.Add(new HtmlFileRecord()
                {
                    Path = relativePath,
                    Title = title
                });
            }

            Logger.Trace($"Adding record to index");
            _index.AddUpdateLuceneIndex(records);
            Logger.Info($"{records.Count} record were added to index");
            callback?.Invoke();
        }

        public void RunReindexTask(string directory, Action callback = null)
        {
            Logger.Info($"Starting indexing task in '{directory}'");
            Task.Factory.StartNew(() => { AddUpdateFilesToIndex(directory, callback); });
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
            Logger.Info($"Searching '{query}' in index");
            if (onComplete == null)
            {
                Logger.Warn($"Missing callback for search query");
                return;
            }

            if (IsEmptyIndex)
            {
                Logger.Info($"Index is empty");
                onComplete(new List<HtmlFileRecord>());
                return;
            }

            // cancel previous search task 
            if (_currentSearchTask != null && !_currentSearchTask.IsCompleted)
            {
                Logger.Trace($"Cancelling previous search task");
                _searchCts?.Cancel();
            }

            _searchCts = new CancellationTokenSource();

            Logger.Trace($"Running new search task");
            _currentSearchTask = Task.Factory.StartNew(() =>
            {
                var results = _index.Search(query).ToList();
                Logger.Info($"Found {results.Count} results for query '{query}'");
                onComplete(results);
            }, _searchCts.Token);
        }

        public HtmlFileRecord Search(string query)
        {
            Logger.Info($"Searching '{query}' in index");
            
            if (IsEmptyIndex)
            {
                Logger.Info($"Index is empty");
                return null;
            }
            
            var results = _index.Search(query).ToList();
            Logger.Info($"Found {results.Count} results for query '{query}'");
            return results.First();
        }

        public void ClearIndex()
        {
            Logger.Info($"Clearing index");
            Directory.Delete(_indexPath, true);
        }
    }
}
