using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShopApi.Models {
    public class RequestPerKey {
        public int Id { get; set; }
        public string Key { get; set; }
        public DateTime? DateTime { get; set; }
        public int Total { get; set; }
    }
}
