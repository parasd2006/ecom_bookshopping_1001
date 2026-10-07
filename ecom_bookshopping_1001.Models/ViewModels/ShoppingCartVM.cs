using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1001.Models.ViewModels
{
    public class    ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ListCart { get; set; }
        public OrderHeader orderHeader { get; set; }
    }
}
