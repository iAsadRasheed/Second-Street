using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using ox.n.cock2.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Net;

namespace ox.n.cock2.Controllers
{
    public class HomeController : Controller
    {
        private Entities db = new Entities();
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.AspNetUser).Include(p => p.AspNetUser1).Include(p => p.ProductClassification).Include(p => p.ProductSubCategory);
            return View(products.ToList());
        }

        public ActionResult Index22()
        {
            var products = db.Products.Include(p => p.AspNetUser).Include(p => p.AspNetUser1).Include(p => p.ProductClassification).Include(p => p.ProductSubCategory);
            return View(products.ToList());
        }

        public PartialViewResult GetProductById(int id)
        {
            var product = db.Products.Where(p => p.ProductSubCategory.CategoryId == id).Include(p => p.ProductSubCategory).Include(p => p.ProductClassification);
            return PartialView(product.ToList());
        }

        public ActionResult GetProductDetailsById(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var productDetail = new
            {
                productId = id,
                categoryName = product.ProductSubCategory.ProductCategory.CategoryName,
                subCategoryName = product.ProductSubCategory.SubCategoryName,
                classificationName = product.ProductClassification?.ClassificationName,
                Price = product.Price,
                DiscountAllowed = product.DiscountAllowed
            };
            return Json(new { success = true, productDetail }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Manually()
        {
            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "Id", "CategoryName");
            return View();
        }

        [HttpGet]
        public JsonResult GetProductCategoryDropdown(int categoryId)
        {
            bool success = true;
            var productSubCategoryEntities = db.ProductSubCategories.Where(p => p.CategoryId == categoryId).ToList();
            var productClassificationEntities = db.ProductClassifications.Where(p => p.CategoryId == categoryId).ToList();
            if (productSubCategoryEntities != null || productClassificationEntities != null)
            {
                var subCategories = productSubCategoryEntities.Select(x => new SelectListItem
                {
                    Text = x.SubCategoryName,
                    Value = x.Id.ToString()
                });

                var productClassifications = productClassificationEntities.Select(x => new SelectListItem
                {
                    Text = x.ClassificationName,
                    Value = x.Id.ToString()
                });

                return Json(new { success, subCategories, productClassifications }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductDetail(int classificationId, int subCategoryId)
        {
            var product = db.Products.FirstOrDefault(p => p.ClassificationId == classificationId && p.SubCategoryId == subCategoryId);
            if (product != null)
            {
                return Json(new { success = true, product.ProductDetails, product.Price, product.DiscountAllowed }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PlaceOrderManually(OrderViewModel orderVM)
        {
            bool success = true;
            var orderEntity = new Order()
            {
                CreatedBy = User.Identity.GetUserId(),
                CreatedDate = DateTime.Now,
                OrderInstructions = orderVM.OrderInstructions,
                CustomerName = orderVM.CustomerName,
                IsDeleted = false
            };

            var orderDetailList = new List<OrderDetail>();
            foreach (var orderItemVM in orderVM.OrderItems)
            {
                var productEntities = db.Products.Where(p => orderItemVM.ProductSubCategoryIds.Contains(p.SubCategoryId) && p.ClassificationId == orderItemVM.ProductClassificationId);

                var productOrderDetailList = new List<ProductOrderDetail>();
                foreach (var product in productEntities)
                {
                    productOrderDetailList.Add(new ProductOrderDetail
                    {
                        ProductId = product.Id
                    });
                }

                orderDetailList.Add(new OrderDetail
                {
                    Discount = orderItemVM.Discount,
                    NumberOfItems = orderItemVM.ItemCount,
                    PricePerItem = productEntities.OrderByDescending(p => p.Price).First().Price,
                    ProductOrderDetails = productOrderDetailList
                });
            }

            orderEntity.OrderDetails = orderDetailList;
            db.Orders.Add(orderEntity);
            int updateStatus = db.SaveChanges();

            var base64String = "";
            if (updateStatus > 0)
            {
                base64String = ExportToPDF(orderEntity.id);
                success = true;
            }
            return Json(new { success = success, id = orderEntity.id, base64String });
        }

        [HttpPost]
        public JsonResult PlaceNewOrder(OrderVM orderModel)
        {
            bool success = true;

            var orderEntity = new Order()
            {
                CustomerName = orderModel.CustomerName,
                OrderInstructions = orderModel.OrderInstructions,
                Discount = orderModel.Discount,
                IsPaid = orderModel.IsPaid,
                IsTakeAway = orderModel.IsTakeAway,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity.GetUserId()
            };

            var orderDetailList = new List<OrderDetail>();
            foreach (var orderProduct in orderModel.Products)
            {
                var productEntity = db.Products.FirstOrDefault(p => p.Id == orderProduct.ProductId);

                var productOrderDetailList = new List<ProductOrderDetail>();
                productOrderDetailList.Add(new ProductOrderDetail
                {
                    ProductId = productEntity.Id
                });

                orderDetailList.Add(new OrderDetail
                {
                    Discount = orderProduct.Discount,
                    NumberOfItems = orderProduct.ItemCount,
                    PricePerItem = productEntity.Price,
                    ProductOrderDetails = productOrderDetailList
                });
            }

            orderEntity.OrderDetails = orderDetailList;
            db.Orders.Add(orderEntity);
            int updateStatus = db.SaveChanges();

            var base64String = "";
            if (updateStatus > 0)
            {
                base64String = ExportToPDF(orderEntity.id);
                success = true;
            }
            return Json(new { success = success, id = orderEntity.id, base64String });
        }

        public ActionResult OrderList()
        {
            var orderEntities = (from o in db.Orders
                                 join od in db.OrderDetails on o.id equals od.OrderId
                                 into odGroup
                                 from od in odGroup.DefaultIfEmpty()
                                 join pod in db.ProductOrderDetails on od.id equals pod.OrderDetailId
                                 into podGroup
                                 from pod in podGroup.DefaultIfEmpty()
                                 join p in db.Products on pod.ProductId equals p.Id
                                 into pGroup
                                 from p in pGroup.DefaultIfEmpty()
                                 join psc in db.ProductSubCategories on p.SubCategoryId equals psc.Id
                                 into pscGroup
                                 from psc in pscGroup.DefaultIfEmpty()
                                 join pc in db.ProductClassifications on p.ClassificationId equals pc.Id
                                 into pcGroup
                                 from pc in pcGroup.DefaultIfEmpty()

                                 select new
                                 {
                                     o,
                                     od,
                                     pod,
                                     p,
                                     psc,
                                     pc
                                 }).ToList()
                                 .GroupBy(e => e.o)
                                 .Select(x => x.Key)
                                 .ToList()
                                 .Select(o => new OrdersListVM
                                 {
                                     OrderKey = "SSC_" + o.id,
                                     NetTotal = o.OrderDetails.Sum(od => od.PricePerItem * od.NumberOfItems - od.Discount),
                                     NoOfItems = o.OrderDetails.Sum(od => od.NumberOfItems)
                                 });
            return View(orderEntities);
        }

        public string ExportToPDF(int id)
        {
            try
            {
                if (true)
                {
                    string mimeTime;
                    string encoding;
                    string fileNameExtrention;

                    string[] streams;
                    Warning[] warnings;
                    byte[] renderdByte;

                    LocalReport localReport = new LocalReport();
                    localReport.EnableExternalImages = true;
                    string reportPath = Server.MapPath(@"~\Report.rdlc");
                    localReport.ReportPath = reportPath;

                    ReportDataSource dataSource = new ReportDataSource();
                    dataSource.Name = "DataSet1";

                    IList<GetOrderDetail_Result> lst = db.GetOrderDetail(id).ToList();
                    dataSource.Value = lst;

                    localReport.DataSources.Add(dataSource);
                    renderdByte = localReport.Render("pdf", "", out mimeTime, out encoding, out fileNameExtrention, out streams, out warnings);
                    string AsBase64String = Convert.ToBase64String(renderdByte);

                    return AsBase64String;
                }
            }
            catch (Exception ex)
            {
                return "404";
            }
        }

    }
}