using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace OfflineWebsiteViewer.Search
{
    public class LuceneIndex<T>
    {
        private readonly string _luceneDir;
        private FSDirectory _directoryTemp;
        private readonly ILuceneMapper<T> _mapper;
        private readonly int _limit;

        private FSDirectory Directory
        {
            get
            {
                if (_directoryTemp == null)
                {
                    var dir = new DirectoryInfo(_luceneDir);
                    dir.Create();
                    _directoryTemp = FSDirectory.Open(dir);
                }

                if (IndexWriter.IsLocked(_directoryTemp))
                    IndexWriter.Unlock(_directoryTemp);

                var lockFilePath = Path.Combine(_luceneDir, "write.lock");

                if (File.Exists(lockFilePath))
                    File.Delete(lockFilePath);

                return _directoryTemp;
            }
        }

        public LuceneIndex(string path, ILuceneMapper<T> mapper, int limit = 10)
        {
            _luceneDir = path;
            _mapper = mapper;
            _limit = limit;
        }

        private void AddToLuceneIndex(T sampleData, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term(_mapper.IdField, _mapper.IdOf(sampleData)));
            writer.DeleteDocuments(searchQuery);

            // add entry to index
            writer.AddDocument(_mapper.ToDocument(sampleData));
        }

        public void AddUpdateLuceneIndex(IEnumerable<T> sampleDatas)
        {
            // init lucene
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entry if any)
                foreach (var sampleData in sampleDatas)
                    AddToLuceneIndex(sampleData, writer);

                // close handles
                analyzer.Close();
            }
        }

        public void AddUpdateLuceneIndex(T sampleData)
        {
            AddUpdateLuceneIndex(new List<T> { sampleData });
        }

        public void ClearLuceneIndexRecord(T item)
        {
            // init lucene
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term(_mapper.IdField, _mapper.IdOf(item)));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
            }
        }

        public bool ClearIndex()
        {
            try
            {
                using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void Optimize()
        {
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
            }
        }


        private IEnumerable<T> _mapLuceneToDataList(IEnumerable<Document> docs)
        {
            return docs.Select(_mapper.FromDocument).ToList();
        }
        private IEnumerable<T> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            return hits.Select(hit => _mapper.FromDocument(searcher.Doc(hit.Doc))).ToList();
        }

        private Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        private IEnumerable<T> _search(string searchQuery, string searchField = "")
        {
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<T>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(Directory, false))
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            {
                // search by single field
                if (!string.IsNullOrEmpty(searchField))
                {
                    var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchField, analyzer);
                    var query = ParseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, _limit).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    return results;
                }
                // search by multiple fields (ordered by RELEVANCE)
                else
                {
                    var parser = new MultiFieldQueryParser
                        (Lucene.Net.Util.Version.LUCENE_30, _mapper.Fields, analyzer);
                    var query = ParseQuery(searchQuery, parser);
                    var hits = searcher.Search
                        (query, null, _limit, Sort.RELEVANCE).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    return results;
                }
            }
        }

        public IEnumerable<T> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input)) return new List<T>();

            var terms = input.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }

        public IEnumerable<T> SearchDefault(string input, string fieldName = "")
        {
            return string.IsNullOrEmpty(input) ? new List<T>() : _search(input, fieldName);
        }

        public IEnumerable<T> GetAllIndexRecords()
        {
            if(!System.IO.Directory.Exists(_luceneDir))
                return new List<T>();

            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<T>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(Directory, false))
            using (var reader = IndexReader.Open(Directory, false))
            {
                var docs = new List<Document>();
                var term = reader.TermDocs();
                while (term.Next())
                    docs.Add(searcher.Doc(term.Doc));
                return _mapLuceneToDataList(docs);
            }
        }
    }
}
