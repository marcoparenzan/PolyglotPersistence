using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using System.Data.Entity.Core.Metadata.Edm;

namespace WebModel
{
    public class ProductCartEntity: TableEntity
    {
        public string ProductDescription { get; set; }

        public int Quantity { get; set; }

        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
