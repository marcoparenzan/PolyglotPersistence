using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebFrontEnd.Models;
using WebModel;
using System.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StackExchange.Redis;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Web.Security;
using System.Text;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WebFrontEnd.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private DocumentClient _client;

        protected DocumentClient Client
        {
            get
            {
                if (_client == null)
                {
                    var endpoint = ConfigurationManager.AppSettings["DocumentDbEndPoint"];
                    var endpointUri = new Uri(endpoint);
                    var authKey = ConfigurationManager.AppSettings["DocumentDbAuthKey"];
                    _client = new DocumentClient(endpointUri, authKey);
                }
                return _client;
            }
        }

        private SearchIndexClient _search;
        protected SearchIndexClient Search
        {
            get
            {
                if (_search == null)
                {
                    var serviceName = ConfigurationManager.AppSettings["SearchServiceName"];
                    var queryKey = ConfigurationManager.AppSettings["SearchQueryKey"];
                    _search = new SearchIndexClient(serviceName, "products", new SearchCredentials(queryKey));
                }
                return _search;
            }
        }

        private CloudBlobClient _blobClient;
        protected CloudBlobClient BlobClient
        {
            get
            {
                if (_blobClient == null)
                {
                    var cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["cloudStorageAccount"]);
                    _blobClient = cloudStorageAccount.CreateCloudBlobClient();
                }
                return _blobClient;
            }
        }

        private CloudTableClient _tableClient;
        protected CloudTableClient TableClient
        {
            get
            {
                if (_tableClient == null)
                {
                    var cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["cloudStorageAccount"]);
                    _tableClient = cloudStorageAccount.CreateCloudTableClient();
                }
                return _tableClient;
            }
        }

        private CloudQueueClient _queueClient;
        protected CloudQueueClient QueueClient
        {
            get
            {
                if (_queueClient == null)
                {
                    var cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["cloudStorageAccount"]);
                    _queueClient = cloudStorageAccount.CreateCloudQueueClient();
                }
                return _queueClient;
            }
        }

        // http://tostring.it/2015/03/05/all-you-need-to-know-about-redis/
        private IDatabase _redisDatabase;
        protected IDatabase RedisDatabase
        {
            get
            {
                if (_redisDatabase == null)
                {
                    var mux = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisConnectionString"]);
                    _redisDatabase = mux.GetDatabase();
                }
                return _redisDatabase;
            }
        }

        protected IOrderedQueryable<ProductDetailDTO> DocumentDbProducts(string continuation = null, int maxItemCount = 10)
        {
            if (!string.IsNullOrWhiteSpace(continuation)) continuation = Encoding.UTF8.GetString(Convert.FromBase64String(continuation));

            var options = new FeedOptions {
                RequestContinuation = continuation,
                MaxItemCount = maxItemCount
            };
            var query = Client.CreateDocumentQuery<ProductDetailDTO>("/dbs/AdventureWorks/colls/documents", options);

            return query;
        }

        public async Task<JsonResult> Suggest(string text)
        {
            var parameters = new SuggestParameters
            {
            };
            var response = await Search.Documents.SuggestAsync(text, "products", parameters);
            var items = response.Results.Select(xx => xx.Text).ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SearchItems(string text, string continuation)
        {
            DocumentSearchResult<ProductIndexDTO> response = null;
            if (!string.IsNullOrWhiteSpace(continuation))
            {
                var continuationToken = JsonConvert.DeserializeObject<SearchContinuationToken>(Encoding.UTF8.GetString(Convert.FromBase64String(continuation)));
                response = await Search.Documents.ContinueSearchAsync<ProductIndexDTO>(continuationToken);
            }
            else
            {
                var parameters = new SearchParameters
                {
                    IncludeTotalResultCount = true
                };
                response = await Search.Documents.SearchAsync<ProductIndexDTO>(text, parameters);
            }
            var items = response.Results.Select(xx => xx.Document).ToList();
            return Json(new ProductPageDTO<ProductIndexDTO>
            {
                Items = items,
                Query = "SearchItems",
                Text = text,
                Continuation = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response.ContinuationToken)))
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> Items(string continuation)
        {
            var query = DocumentDbProducts(continuation)
                .Select(xx => new { id = xx.id, Name = xx.Name, ProductNumber = xx.ProductNumber })
                .AsDocumentQuery()
            ;

            var response = await query.ExecuteNextAsync();

            var items =
                response
                .ToList()
                .Select(xx => new ProductIndexDTO { id = Guid.Parse(xx.id), Name = xx.Name, ProductNumber = xx.ProductNumber })
                .ToList()
                ;

            return Json(new ProductPageDTO<ProductIndexDTO> {
                Items = items,
                Query = "Items",
                Continuation = Convert.ToBase64String(Encoding.UTF8.GetBytes(response.ResponseContinuation))
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(Guid id, int quantity = 1)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.quantity = quantity;

            var key = string.Format("product_detail_{0}", id);
            if (!RedisDatabase.KeyExists(key))
            {
                // ...
                var productDetailDTO = DocumentDbProducts().Where(xx => xx.id == id).ToList();
                if (productDetailDTO == null)
                {
                    return HttpNotFound();
                }
                var json = JsonConvert.SerializeObject(productDetailDTO);
                RedisDatabase.StringSet(key, json, TimeSpan.FromHours(4));
                RedisDatabase.KeyDelete(key);
                return View(productDetailDTO[0]);
            }
            else
            {
                // ...
                var json = RedisDatabase.StringGet(key);
                var productDetailDto = JsonConvert.DeserializeObject<ProductDetailDTO>(json);
                return View(productDetailDto);
            }
        }

        public async Task<ActionResult> Images(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // ...
            var productDetailDTO = DocumentDbProducts().Where(xx => xx.id == id).ToList();
            // ...
            if (productDetailDTO == null)
            {
                return HttpNotFound();
            }
            var container = BlobClient.GetContainerReference("webfrontendimages");
            var blob = container.GetBlobReference(productDetailDTO[0].ThumbnailPhotoFileName);
            Response.ContentType = blob.Properties.ContentType;
            await blob.DownloadToStreamAsync(Response.OutputStream);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart(Guid id, int quantity)
        {
            var shoppingCart = TableClient.GetTableReference("shoppingcart");
            shoppingCart.CreateIfNotExists();

            var retrieve = TableOperation.Retrieve<ProductCartEntity>(UserSession(), id.ToString());
            var result = shoppingCart.Execute(retrieve);
            var item = (ProductCartEntity)result.Result;
            if (item != null)
            {
                item.Quantity = quantity;
                item.TotalPrice = quantity * item.UnitPrice;
            }
            else
            {
                var product = DocumentDbProducts().Where(xx => xx.id == id).ToList().First();
                item = new ProductCartEntity
                {
                    PartitionKey = UserSession()
                    ,
                    RowKey = id.ToString()
                    ,
                    CreateDate = DateTime.Now
                    ,
                    ProductDescription = product.Name
                    ,
                    Currency = "EUR"
                    ,
                    UnitPrice = (double) product.ListPrice
                    ,
                    TotalPrice = quantity * (double)product.ListPrice
                    ,
                    Quantity = quantity
                };
            }

            var upsert = TableOperation.InsertOrReplace(item);
            shoppingCart.Execute(upsert);

            return RedirectToAction("ShoppingCart");
        }

        public async Task<ActionResult> RemoveFromCart(Guid id)
        {
            RemoveFromCartById(id);

            return RedirectToAction("ShoppingCart");
        }

        private void RemoveFromCartById(string id)
        {
            RemoveFromCartById(Guid.Parse(id));
        }

        private void RemoveFromCartById(Guid id)
        {
            var shoppingCart = TableClient.GetTableReference("shoppingcart");
            shoppingCart.CreateIfNotExists();

            var retrieve = TableOperation.Retrieve<ProductCartEntity>(UserSession(), id.ToString());
            var result = shoppingCart.Execute(retrieve);
            var item = (ProductCartEntity)result.Result;
            if (item != null)
            {
                var delete = TableOperation.Delete(item);
                shoppingCart.Execute(delete);
            }
        }

        public ActionResult ShoppingCart()
        {
            return View();
        }

        public JsonResult ShoppingCartItems()
        {
            return Json(new ProductPageDTO<ProductCartDTO>
            {
                Items = ShoppingCartContent(),
                Query = "ShoppingCartItems"
            }, JsonRequestBehavior.AllowGet);
        }

        private List<ProductCartDTO> ShoppingCartContent()
        {
            var shoppingCart = TableClient.GetTableReference("shoppingcart");
            shoppingCart.CreateIfNotExists();

            var query = (TableQuery<ProductCartEntity>)shoppingCart.CreateQuery<ProductCartEntity>()
                .Where(xx => xx.PartitionKey == UserSession());

            var items = shoppingCart.ExecuteQuery(query)
                .Select(xx => new ProductCartDTO
                {
                    Currency = xx.Currency
                    ,
                    ProductDescription = xx.ProductDescription
                    ,
                    ProductId = xx.RowKey
                    ,
                    Quantity = xx.Quantity
                    ,
                    TotalPrice = (decimal)xx.TotalPrice
                    ,
                    UnitPrice = (decimal)xx.UnitPrice
                }).ToList();
            return items;
        }

        public ActionResult SubmitOrder()
        {
            var ordersQueue = QueueClient.GetQueueReference("orders");
            ordersQueue.CreateIfNotExists();

            var userName = User.Identity.Name;
            var user = (new ApplicationDbContext()).Users.FirstOrDefault(s => s.Email == userName);

            var messageContent = new 
            {
                OrderId = UserSession(),
                CustomerId = user.CustomerId,
                Username = user.UserName,
                Items = ShoppingCartContent()
            };

            var message = new CloudQueueMessage(JsonConvert.SerializeObject(messageContent))
            {
            };

            ordersQueue.AddMessage(message);

            CloseUserSession();

            messageContent.Items.ForEach(xx => {
                RemoveFromCartById(xx.ProductId);
            });

            return RedirectToAction("Index");
        }

        private string UserSession()
        {
            string userSession = null;
            var cookie = Request.Cookies["userSession"];
            if (cookie == null)
            {
                userSession = Guid.NewGuid().ToString();
                Response.Cookies.Add(new HttpCookie("userSession", userSession));
            }
            else
            {
                userSession = cookie.Value;
                if (string.IsNullOrWhiteSpace(userSession))
                {
                    userSession = Guid.NewGuid().ToString();
                    Response.Cookies.Add(new HttpCookie("userSession", userSession));
                }
            }
            return userSession;
        }

        private void CloseUserSession()
        {
            var cookie = Request.Cookies["userSession"];
            if (cookie != null)
            {
                Request.Cookies.Remove("userSession");
            }
            cookie = Response.Cookies["userSession"];
            if (cookie != null)
            {
                Response.Cookies.Remove("userSession");
            }
        }
    }
}
