using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebModel
{
    public abstract class WebModelDTO<TDTO>
        where TDTO: WebModelDTO<TDTO>
    {
        public string Type { get; set; }

        public WebModelDTO()
        {
            this.Type = this.GetType().Name;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static TDTO FromJson(string json)
        {
            return JsonConvert.DeserializeObject<TDTO>(json);
        }
    }
}
