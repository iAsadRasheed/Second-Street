using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ox.n.cock2.Models
{
    public class OrderVM
    {
        public string CustomerName { get; set; }
        public string OrderInstructions { get; set; }
        public decimal Discount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsTakeAway { get; set; }
        public List<ProductVM> Products { get; set; }
    }
}