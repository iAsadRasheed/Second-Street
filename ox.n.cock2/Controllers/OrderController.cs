using ox.n.cock2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ox.n.cock2.Controllers
{
    public class OrderController : Controller
    {
        private Entities _dbContext = new Entities();
        const int hoursToAdd = -6;

        public ActionResult Index(string strDate)
        {
            ViewBag.Date = strDate;
            return View();
        }
        public PartialViewResult _Orders(string strDate)
        {
            DateTime dateTime;
            var isValidDate = DateTime.TryParseExact(strDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

            return PartialView(GetOrders(isValidDate ? (DateTime?)dateTime : null));
        }

        public PartialViewResult RecentOrders()
        {
            var response = new List<OrdersByDatesVM>();
            var ordersByDate = GetOrders(DateTime.Now.AddHours(-6).AddDays(-22).Date).FirstOrDefault();

            if (ordersByDate != null)
            {
                var orderList = ordersByDate
                                .OrdersListVM
                                .GroupBy(ol => ol.IsPaid)
                                .SelectMany(g => g.OrderByDescending(t => t.OrderKey)
                                .Take(5));

                response.Add(new OrdersByDatesVM
                {
                    OrderDate = ordersByDate.OrderDate,
                    OrdersListVM = orderList.ToList()
                });
            }

            return PartialView("_Orders", response);
        }

        private IEnumerable<OrdersByDatesVM> GetOrders(DateTime? dateTime)
        {
            return _dbContext.Orders.ToList().Where(o => dateTime == null ? true : o.CreatedDate.AddHours(hoursToAdd).Date == dateTime.Value.Date)
                                       .GroupBy(g => g.CreatedDate.AddHours(hoursToAdd).Date)
                                       .Select(g => new OrdersByDatesVM
                                       {
                                           OrderDate = g.Key.AddHours(-hoursToAdd),
                                           OrdersListVM = g.Select(order => new OrdersListVM
                                           {
                                               Discount = order.Discount + order.OrderDetails.Sum(od => od.Discount),
                                               IsDeleted = order.IsDeleted,
                                               IsPaid = order.IsPaid,
                                               NetTotal = order.OrderDetails.Sum(od => od.PricePerItem * od.NumberOfItems - od.Discount) - order.Discount,
                                               NoOfItems = order.OrderDetails.Sum(od => od.NumberOfItems),
                                               OrderKey = "SSC_" + order.id
                                           })
                                           .OrderByDescending(o => o.OrderKey)
                                           .ToList()
                                       })
                                       .OrderByDescending(o => o.OrderDate);
        }

        public ActionResult Details(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                int orderId = Convert.ToInt32(id.Substring(4));

                var orderDetailModel = (from order in _dbContext.Orders.Where(o => o.id == orderId)

                                        join orderDetail in _dbContext.OrderDetails on order.id equals orderDetail.OrderId
                                        into orderDetailGroup
                                        from orderDetail in orderDetailGroup.DefaultIfEmpty()

                                        join productOrderDetail in _dbContext.ProductOrderDetails on orderDetail.id equals productOrderDetail.OrderDetailId
                                        into productOrderDetailGroup
                                        from productOrderDetail in productOrderDetailGroup.DefaultIfEmpty()

                                        join product in _dbContext.Products on productOrderDetail.ProductId equals product.Id
                                        into productGroup
                                        from product in productGroup.DefaultIfEmpty()

                                        join productSubCategory in _dbContext.ProductSubCategories on product.SubCategoryId equals productSubCategory.Id
                                        into productSubCategoryGroup
                                        from productSubCategory in productSubCategoryGroup.DefaultIfEmpty()

                                        join productClassification in _dbContext.ProductClassifications on product.ClassificationId equals productClassification.Id
                                        into productClassificationGroup
                                        from productClassification in productClassificationGroup.DefaultIfEmpty()

                                        select new
                                        {
                                            order,
                                            orderDetail,
                                            productOrderDetail,
                                            product,
                                            productSubCategory,
                                            productClassification
                                        }).ToList()
                                     .GroupBy(e => e.order)
                                     .Select(x => x.Key)
                                     .Select(order => new OrderDetailVM
                                     {
                                         CustomerName = order.CustomerName,
                                         OrderInstructions = order.OrderInstructions,
                                         CreatedDate = order.CreatedDate,
                                         CreatedBy = order.AspNetUser.Email,
                                         Discount = order.Discount,
                                         IsPaid = order.IsPaid,
                                         IsTakeAway = order.IsTakeAway,
                                         IsDeleted = order.IsDeleted,
                                         GrossTotal = order.OrderDetails.Sum(od => od.PricePerItem * od.NumberOfItems - od.Discount),

                                         OrderItemsVM = order.OrderDetails.Select(orderDetail => new OrderItemsVM
                                         {
                                             CategoryName = orderDetail.ProductOrderDetails.First().Product.ProductSubCategory.ProductCategory.CategoryName,
                                             SubCategoryName = orderDetail.ProductOrderDetails.First().Product.ProductSubCategory.SubCategoryName,
                                             ClassificationName = orderDetail.ProductOrderDetails.First().Product.ProductClassification != null
                                                ? orderDetail.ProductOrderDetails.First().Product.ProductClassification.ClassificationName
                                                : String.Empty,
                                             ProductDetail = orderDetail.ProductOrderDetails.First().Product.ProductDetails,
                                             PricePerItem = orderDetail.PricePerItem,
                                             ItemsCount = orderDetail.NumberOfItems,
                                             Discount = orderDetail.Discount

                                         }).ToList()

                                     }).FirstOrDefault();

                return View(orderDetailModel);
            }

            return View(new OrderDetailVM());
        }

        [HttpPost]
        public bool ToggleOrderStatus(string strOrderId)
        {
            if (!string.IsNullOrWhiteSpace(strOrderId))
            {
                var orderId = Convert.ToInt32(strOrderId.Substring(strOrderId.IndexOf('_') + 1));

                var orderEntity = _dbContext.Orders.FirstOrDefault(o => o.id == orderId);
                if (orderEntity != null)
                {
                    orderEntity.IsPaid = !orderEntity.IsPaid;
                    var flag = _dbContext.SaveChanges();

                    return flag > 0;
                }
            }
            return false;
        }
    }
}