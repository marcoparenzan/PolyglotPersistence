using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebModel
{
    public class ProductDetailDTO: WebModelDTO<ProductDetailDTO>
    {
        public Guid id { get; set; }

        public string Name { get; set; }

        public string ProductNumber { get; set; }

        public string Color { get; set; }

        public decimal StandardCost { get; set; }

        public decimal ListPrice { get; set; }

        public string Size { get; set; }

        public decimal? Weight { get; set; }

        public DateTime SellStartDate { get; set; }

        public DateTime? SellEndDate { get; set; }

        public DateTime? DiscontinuedDate { get; set; }

        public string ThumbnailPhotoFileName { get; set; }

        public DateTime ModifiedDate { get; set; }

        public ProductCategoryDTO ProductCategory { get; set; }

        public ProductModelDTO ProductModel { get; set; }
    }
}
