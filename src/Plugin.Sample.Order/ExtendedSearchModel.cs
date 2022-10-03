using Sitecore.Commerce.Plugin.Search;
using SolrNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Sample.Order
{
    public class ExtendedSearchModel
    {        
        public SolrQueryResults<Document> ExtendedDocument { get; set; }
    }

    public class ExtendedDocumentSearchModel
    {
        public List<Document> Documents { get; set; }
    }
}
