using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ox.n.cock2.Models;

namespace ox.n.cock2.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private Entities db = new Entities();

        // GET: Product
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.AspNetUser).Include(p => p.AspNetUser1).Include(p => p.ProductClassification).Include(p => p.ProductSubCategory).OrderBy(p => p.ProductSubCategory.ProductCategory.Id);
            return View(products.ToList());
        }

        // GET: Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "Id", "CategoryName");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SubCategoryId,ClassificationId,ProductName,DiscountAllowed,Price")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedBy = User.Identity.GetUserId();
                product.CreatedDate = DateTime.Now;
                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "Id", "CategoryName");
            ViewBag.ClassificationId = new SelectList(db.ProductClassifications, "Id", "ClassificationName");
            ViewBag.SubCategoryId = new SelectList(db.ProductSubCategories, "Id", "SubCategoryName");
            return View(product);
        }

        // GET: Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "Id", "CategoryName", product.ProductSubCategory.CategoryId);            
            ViewBag.ClassificationId = new SelectList(db.ProductClassifications.Where(p => p.CategoryId == product.ProductSubCategory.CategoryId), "Id", "ClassificationName", product.ClassificationId);
            ViewBag.SubCategoryId = new SelectList(db.ProductSubCategories.Where(p => p.CategoryId == product.ProductSubCategory.CategoryId), "Id", "SubCategoryName", product.SubCategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SubCategoryId,ClassificationId,ProductDetails,DiscountAllowed,Price")] Product product)
        {
            Product productEntity = db.Products.Find(product.Id);
            if (productEntity == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                productEntity.SubCategoryId = product.SubCategoryId;
                productEntity.ClassificationId = product.ClassificationId;
                productEntity.ProductDetails = product.ProductDetails;
                productEntity.DiscountAllowed = product.DiscountAllowed;
                productEntity.Price = product.Price;
                productEntity.UpdatedBy = User.Identity.GetUserId();
                productEntity.UpdatedDate = DateTime.Now;
                db.Entry(productEntity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "Id", "CategoryName", productEntity.ProductSubCategory.CategoryId);
            ViewBag.ClassificationId = new SelectList(db.ProductClassifications.Where(p => p.CategoryId == productEntity.ProductSubCategory.CategoryId), "Id", "ClassificationName", product.ClassificationId);
            ViewBag.SubCategoryId = new SelectList(db.ProductSubCategories.Where(p => p.CategoryId == productEntity.ProductSubCategory.CategoryId), "Id", "SubCategoryName", product.SubCategoryId);
            return View(product);
        }

        // GET: Product/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult GetProductClassificationDropdown(int categoryId)
        {
            var productClassification = db.ProductClassifications.Where(p => p.CategoryId == categoryId);
            if (productClassification != null)
            {
                var classification = productClassification.Select(x => new SelectListItem
                {
                    Text = x.ClassificationName,
                    Value = x.Id.ToString()
                });
                return Json(new { success = true, classification }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
