using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebModel
{
    public class ProductPageDTO<TItem> : WebModelDTO<ProductPageDTO<TItem>>
    {
        public string Continuation { get; set; }
        public List<TItem> Items { get; set; }
        public string Query { get; set; }
        public string Text { get; set; }
    }
}
