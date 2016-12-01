using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFrontEnd.Models
{
    public class PageDTO<T>
    {
        public List<T> Items { get; set; }
        public string Continuation { get; set; }
    }
}
