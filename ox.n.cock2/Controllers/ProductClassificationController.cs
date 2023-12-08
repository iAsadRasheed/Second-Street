using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ox.n.cock2.Models;

namespace ox.n.cock2.Controllers
{
    [Authorize]
    public class ProductClassificationController : Controller
    {
        private Entities db = new Entities();

        // GET: ProductType
        public ActionResult Index()
        {
            var productClassifications = db.ProductClassifications.Include(p => p.AspNetUser).Include(p => p.AspNetUser1).Include(p => p.ProductCategory);
            return View(productClassifications.ToList());
        }

        // GET: ProductClassifications/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductClassification productClassification = db.ProductClassifications.Find(id);
            if (productClassification == null)
            {
                return HttpNotFound();
            }
            return View(productClassification);
        }

        // GET: ProductClassifications/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName");
            return View();
        }

        // POST: ProductClassifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryId,ClassificationName")] ProductClassification subClassification)
        {
            if (ModelState.IsValid)
            {
                subClassification.CategoryId = subClassification.CategoryId;
                subClassification.CreatedBy = User.Identity.GetUserId();
                subClassification.CreatedDate = DateTime.Now;
                db.ProductClassifications.Add(subClassification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName", subClassification.CategoryId);
            return View(subClassification);
        }

        // GET: ProductClassifications/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductClassification productClassification = db.ProductClassifications.Find(id);
            if (productClassification == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName", productClassification.CategoryId);
            return View(productClassification);
        }

        // POST: ProductClassifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CategoryId,ClassificationName")] ProductClassification productClassification)
        {
            if (ModelState.IsValid)
            {
                ProductClassification productClassificationEntity = db.ProductClassifications.Find(productClassification.Id);
                if (productClassificationEntity == null)
                {
                    return HttpNotFound();
                }

                productClassificationEntity.CategoryId = productClassification.CategoryId;
                productClassificationEntity.ClassificationName = productClassification.ClassificationName;
                productClassificationEntity.UpdatedBy = User.Identity.GetUserId();
                productClassificationEntity.UpdatedDate = DateTime.Now;
                db.Entry(productClassificationEntity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName", productClassification.CategoryId);
            return View(productClassification);
        }

        // GET: ProductClassifications/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductClassification productClassification = db.ProductClassifications.Find(id);
            if (productClassification == null)
            {
                return HttpNotFound();
            }
            return View(productClassification);
        }

        // POST: ProductClassifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductClassification productClassification = db.ProductClassifications.Find(id);
            db.ProductClassifications.Remove(productClassification);
            db.SaveChanges();
            return RedirectToAction("Index");
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