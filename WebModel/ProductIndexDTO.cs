using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebModel
{
    public class ProductIndexDTO: WebModelDTO<ProductIndexDTO>
    {
        public Guid id { get; set; }

        public string Name { get; set; }

        public string ProductNumber { get; set; }
        public string ProductModel { get; set; }
        public string ProductCategory { get; set; }

        [JsonProperty("@search.score")]
        public decimal SearchScore { get; set; }
    }
}
