using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShopApi.Models {
    public class User {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? CoffeeId { get; set; }
        public Coffee FavouriteCoffee { get; set; }
    }
}
