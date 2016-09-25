using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace OfflineWebsiteViewer
{
    class LuceneService
    {
        public const string SearchIndexPath = "SearchIndex";

        public bool IndexExists => System.IO.Directory.Exists(Path.Combine(_project.ProjectPath, SearchIndexPath));

        private readonly Regex _titleRegex;
        private readonly OfflineWebResourceProject _project;

        public LuceneService(OfflineWebResourceProject project)
        {
            _project = project;
            _titleRegex = new Regex(@"<title>(.*?)</title>", RegexOptions.Compiled);
        }

        public void CreateIndex()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var indexDirectory = new SimpleFSDirectory(new DirectoryInfo(Path.Combine(_project.ProjectPath, SearchIndexPath)));
            using (var writer = new IndexWriter(indexDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();
                var files = System.IO.Directory.EnumerateFiles(_project.ProjectPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".htm", StringComparison.OrdinalIgnoreCase));
                foreach (var file in files)
                {
                    writer.AddDocument(MapHtmlFile(_project.ProjectPath, GetRelativePath(file, _project.ProjectPath)));
                }
            }
        }

        private Document MapHtmlFile(string basePath, string relativePath)
        {
            var text = File.ReadAllText(Path.Combine(basePath, relativePath));

            var result = _titleRegex.Match(text);
            var title = result.Groups[1].Value;
            Console.WriteLine($"Indexing {relativePath}: {title}");

            var document = new Document();
            document.Add(new Field("Path", relativePath, Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("Title", title, Field.Store.YES, Field.Index.NOT_ANALYZED));
            return document;
        }

        private static Query GetQuery(string query)
        {
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                var parser = new QueryParser(Version.LUCENE_30, "Title", analyzer);
                return parser.Parse(query);
            }
        }

        public List<Tuple<string, string>> Search(string query, int limit, out int count)
        {
            using (var directory = GetDirectory())
            using (var searcher = new IndexSearcher(directory))
            {
                var docs = searcher.Search(GetQuery(query), limit);
                count = docs.TotalHits;

                var results = new List<Tuple<string, string>>();
                foreach (var scoreDoc in docs.ScoreDocs)
                {
                    var doc = searcher.Doc(scoreDoc.Doc);
                    results.Add(new Tuple<string,string>(doc.Get("Path"), doc.Get("Title")));
                }

                return results;
            }
        }

        private Lucene.Net.Store.Directory GetDirectory()
        {
            return new SimpleFSDirectory(new DirectoryInfo(Path.Combine(_project.ProjectPath, SearchIndexPath)));
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
    }
}
