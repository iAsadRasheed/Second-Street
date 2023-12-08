using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ox.n.cock2.Models
{
    public class OrderViewModel
    {
        public string CustomerName { get; set; }
        public string OrderInstructions { get; set; }
        public List<OrderItemsViewModel> OrderItems { get; set; }
    }

    public class OrderItemsViewModel
    {
        public List<int> ProductSubCategoryIds { get; set; }
        public int ProductClassificationId { get; set; }
        public int ItemCount { get; set; }
        public int Discount { get; set; }
        public int AdditionalCharges { get; set; }
    }
}