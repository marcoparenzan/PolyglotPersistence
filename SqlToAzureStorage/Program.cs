using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using SqlDataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["cloudStorageAccount"]);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var webfrontendimages = blobClient.GetContainerReference("webfrontendimages");
            webfrontendimages.CreateIfNotExists();

            var thumbnailPhotoFileNames = new HashSet<string>();

            var dbContext = new AdventureWorksLTDataModel();
            var products = dbContext.Product.ToList();
            var i = 1;
            foreach (var product in products)
            {
                var thumbnailPhotoFileName = product.ThumbnailPhotoFileName;
                if (thumbnailPhotoFileNames.Contains(thumbnailPhotoFileName)) continue;

                thumbnailPhotoFileNames.Add(thumbnailPhotoFileName);
                var blob = webfrontendimages.GetAppendBlobReference(thumbnailPhotoFileName);
                blob.UploadFromByteArray(product.ThumbNailPhoto, 0, product.ThumbNailPhoto.Length);
                blob.Properties.CacheControl = "x-ms-blob-cache-control: public, max-age=31556926";

                Console.Write("{0}", thumbnailPhotoFileName);
                Console.WriteLine("...done");
                i++;
            }
        }
    }
}
