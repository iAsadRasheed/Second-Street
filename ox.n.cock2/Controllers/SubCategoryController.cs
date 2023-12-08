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
    public class SubCategoryController : Controller
    {
        private Entities db = new Entities();

        // GET: ProductSubCategories
        public ActionResult Index()
        {
            var productSubCategories = db.ProductSubCategories.Include(p => p.AspNetUser).Include(p => p.AspNetUser1).Include(p => p.ProductCategory);
            return View(productSubCategories.OrderBy(psc => psc.ProductCategory.CategoryName).ToList());
        }

        // GET: ProductSubCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSubCategory productSubCategory = db.ProductSubCategories.Find(id);
            if (productSubCategory == null)
            {
                return HttpNotFound();
            }
            return View(productSubCategory);
        }

        // GET: ProductSubCategories/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName");
            return View();
        }

        // POST: ProductSubCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryId,SubCategoryName")] ProductSubCategory subCategory)
        {
            if (ModelState.IsValid)
            {
                subCategory.CreatedBy = User.Identity.GetUserId();
                subCategory.CreatedDate = DateTime.Now;
                db.ProductSubCategories.Add(subCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // GET: ProductSubCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSubCategory productSubCategory = db.ProductSubCategories.Find(id);
            if (productSubCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName", productSubCategory.CategoryId);
            return View(productSubCategory);
        }

        // POST: ProductSubCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CategoryId,SubCategoryName")] ProductSubCategory subCategory)
        {
            if (ModelState.IsValid)
            {
                ProductSubCategory subCategoryEntity = db.ProductSubCategories.Find(subCategory.Id);
                if (subCategoryEntity == null)
                {
                    return HttpNotFound();
                }

                subCategoryEntity.CategoryId = subCategory.CategoryId;
                subCategoryEntity.SubCategoryName = subCategory.SubCategoryName;
                subCategoryEntity.UpdatedBy = User.Identity.GetUserId();
                subCategoryEntity.UpdatedDate = DateTime.Now;
                db.Entry(subCategoryEntity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.ProductCategories, "id", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // GET: ProductSubCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSubCategory productSubCategory = db.ProductSubCategories.Find(id);
            if (productSubCategory == null)
            {
                return HttpNotFound();
            }
            return View(productSubCategory);
        }

        // POST: ProductSubCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductSubCategory productSubCategory = db.ProductSubCategories.Find(id);
            db.ProductSubCategories.Remove(productSubCategory);
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
