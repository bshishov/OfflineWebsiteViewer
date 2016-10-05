using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;

namespace OfflineWebsiteViewer.Search
{
    class HtmlFileMapper : ILuceneMapper<HtmlFileRecord>
    {
        public static readonly string[] F = { "Path", "Title" };

        public string[] Fields => F;

        public string IdField => F[0];

        public string IdOf(HtmlFileRecord obj) => obj.Path;

        public HtmlFileRecord FromDocument(Document doc)
        {
            return new HtmlFileRecord
            {
                Path = doc.Get(F[0]),
                Title = doc.Get(F[1]),
            };
        }

        public Document ToDocument(HtmlFileRecord record)
        {
            var doc = new Document();

            doc.Add(new Field(F[0], record.Path, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(F[1], record.Title, Field.Store.YES, Field.Index.ANALYZED));
            

            return doc;
        }
    }
}
