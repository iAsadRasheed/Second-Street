using System;
using System.Collections.Generic;

namespace ox.n.cock2.Models
{
    public class OrdersByDatesVM
    {
        public DateTime OrderDate { get; set; }
        public List<OrdersListVM> OrdersListVM { get; set; }
    }

    public class OrdersListVM
    {
        public string OrderKey { get; set; }
        public int NoOfItems { get; set; }
        public decimal NetTotal { get; set; }
        public decimal Discount { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPaid { get; set; }
    }
}