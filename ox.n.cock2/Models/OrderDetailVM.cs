using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ox.n.cock2.Models
{
    public class OrderDetailVM
    {
        public string CustomerName { get; set; }
        public string OrderInstructions { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal Discount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsTakeAway { get; set; }
        public bool IsDeleted { get; set; }
        public List<OrderItemsVM> OrderItemsVM { get; set; }
    }

    public class OrderItemsVM
    {
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string ClassificationName { get; set; }
        public string ProductDetail { get; set; }
        public int ItemsCount { get; set; }
        public decimal PricePerItem { get; set; }
        public decimal Discount { get; set; }
    }
}