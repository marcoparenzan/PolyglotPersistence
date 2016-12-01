using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebModel
{
    public class ProductCartDTO: WebModelDTO<ProductCartDTO>
    {
        public string ProductId { get; set; }

        public string ProductDescription { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; }
    }
}
