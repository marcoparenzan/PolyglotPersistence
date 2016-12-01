using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Partitioning;
using SqlDataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebModel;

namespace SqlToDocumentDb
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var endpoint = ConfigurationManager.AppSettings["DocumentDbEndPoint"];
            var endpointUri = new Uri(endpoint);
            var authKey = ConfigurationManager.AppSettings["DocumentDbAuthKey"];
            var client = new DocumentClient(endpointUri, authKey);

            //var adventureWorksLT =
            //    client.CreateDatabaseQuery()
            //    .Where(xx => xx.Id == "AdventureWorksLT")
            //    .ToList()
            //    .FirstOrDefault();

            //var collectionId = "documents";
            //var documents =
            //    client.CreateDocumentCollectionQuery(adventureWorksLT.SelfLink)
            //    .Where(xx => xx.Id == collectionId)
            //    .ToList()
            //    .FirstOrDefault();

            //var collectionLink = documents.SelfLink;

            var databaseUri = new Uri("/dbs/AdventureWorks", UriKind.Relative);
            var documents =
                client.CreateDocumentCollectionQuery(databaseUri)
                .Where(xx => xx.Id == "documents")
                .ToList()
                .FirstOrDefault();
            documents.IndexingPolicy.IndexingMode = IndexingMode.Lazy;
            documents = await client.ReplaceDocumentCollectionAsync(documents);

            RangePartitionResolver<string> rangeResolver = new RangePartitionResolver<string>(
                p => ((ProductDetailDTO)p).ProductModel.Name,
                new Dictionary<Range<string>, string>()
                {
                    { new Range<string>("A", "M\uffff"), "/dbs/AdventureWorksLT/colls/productsA-M" },
                    { new Range<string>("N", "Z\uffff"), "/dbs/AdventureWorksLT/colls/productsN-Z" },
                });
            client.PartitionResolvers["/dbs/AdventureWorks"] = rangeResolver;

            var dbContext = new AdventureWorksLTDataModel();
            var products = dbContext.Product.ToDTO().ToList();
            var i = 1;
            foreach (var productDto in products)
            {
                var response = await client.CreateDocumentAsync(documents.SelfLink, productDto);
                //var response = await client.CreateDocumentAsync("/dbs/AdventureWorks", productDto);
                Console.Write("{0}", response.Resource.SelfLink);
                Console.WriteLine("...done");
                i++;
            }

            documents.IndexingPolicy.IndexingMode = IndexingMode.Consistent;
            documents.IndexingPolicy.IncludedPaths.Clear();
            // "root"
            var rootPath = new IncludedPath { Path = "/" };
            rootPath.Indexes.Add(new HashIndex(DataType.String));
            rootPath.Indexes.Add(new HashIndex(DataType.Number));
            documents.IndexingPolicy.IncludedPaths.Add(rootPath);
            // Type
            var typePath = new IncludedPath { Path = "/Type/?" };
            typePath.Indexes.Add(new HashIndex(DataType.String));
            documents.IndexingPolicy.IncludedPaths.Add(typePath);
            // excluded
            documents.IndexingPolicy.ExcludedPaths.Clear();
            // ProductCategory
            documents.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/ProductCategory/*" });
            var productCategoryIdPath = new IncludedPath { Path = "/ProductCategory/Id/?" };
            productCategoryIdPath.Indexes.Add(new HashIndex(DataType.String));
            documents.IndexingPolicy.IncludedPaths.Add(productCategoryIdPath);
            // ProductModel
            documents.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/ProductModel/*" });
            var productModelIdPath = new IncludedPath { Path = "/ProductModel/Id/?" };
            productModelIdPath.Indexes.Add(new HashIndex(DataType.String));
            documents.IndexingPolicy.IncludedPaths.Add(productModelIdPath);
            // make effective
            documents = await client.ReplaceDocumentCollectionAsync(documents);
        }
    }
}
