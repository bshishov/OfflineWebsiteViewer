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
    public class HtmlFileIndex
    {
        public bool IsEmptyIndex => !_index.GetAllIndexRecords().Any();
        public int Count => _index.GetAllIndexRecords().Count();

        private readonly string _indexPath;
        private readonly LuceneIndex<HtmlFileRecord> _index;
        private readonly Regex _titleRegex;
        private CancellationTokenSource _searchCts;
        private Task _currentSearchTask;

        public HtmlFileIndex(string indexDirectory)
        {
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
                Console.WriteLine($"Adding to index {relativePath}: {title}");

                records.Add(new HtmlFileRecord()
                {
                    Path = relativePath,
                    Title = title
                });
            }

            _index.AddUpdateLuceneIndex(records);
            callback?.Invoke();
        }

        public void RunReindexTask(string directory, Action callback = null)
        {
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
            if(onComplete == null)
                return;

            if (IsEmptyIndex)
            {
                onComplete(new List<HtmlFileRecord>());
                return;
            }

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

        public void ClearIndex()
        {
            Directory.Delete(_indexPath, true);
        }
    }
}
