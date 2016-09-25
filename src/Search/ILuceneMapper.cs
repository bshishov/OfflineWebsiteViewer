using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;

namespace OfflineWebsiteViewer.Search
{
    public interface ILuceneMapper<T>
    {
        string IdField { get; }
        string[] Fields { get; }
        string IdOf(T obj);
        Document ToDocument(T obj);
        T FromDocument(Document doc);
    }
}
