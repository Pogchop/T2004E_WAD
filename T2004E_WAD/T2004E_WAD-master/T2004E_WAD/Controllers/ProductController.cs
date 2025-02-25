﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using T2004E_WAD.Context;
using T2004E_WAD.Models;
using System.Dynamic;

namespace T2004E_WAD.Controllers
{
    public class ProductController : Controller
    {
        private DataContext db = new DataContext();

        // GET: Product
        public ActionResult Index(string search, string sortOrder, string categoryId)
        {
            ViewBag.CategoryId = 0;
            string sort = !String.IsNullOrEmpty(sortOrder) ? sortOrder : "asc";
            /*var products = db.Products.Where(p=>p.Name.Equals(search)).OrderBy(p => p.Name).ToList(); // OrderBy : asc ascending OrderBy : desc 
            if(search == null)
            {
                products = db.Products.OrderBy(p => p.Name).ToList();
            }
            return View(products);*/

            var products = from p in db.Products select p;
            if(!String.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search));
            }
            switch(sort)
            {
                case "asc": products = products.OrderBy(p => p.Name);break;
                case "desc": products = products.OrderByDescending(p => p.Name);break;
            }
            //loc theo category
            if(!String.IsNullOrEmpty(categoryId))
            {
                var catId = Convert.ToInt32(categoryId);
                products = products.Where(p => p.CategoryID == catId);
                ViewBag.CategoryId = catId;
            }
            var categories = db.Categories.ToList();
            dynamic data = new ExpandoObject();
            data.Products = products;
            data.Categories = categories;
            return View(data);
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

        public ActionResult AddToCart(int? id, int? qty)
        {
            try
            {
                Product product = db.Products.Find(id);
                if(product == null)
                {
                    return HttpNotFound();
                }
                //them vao gio hang
                CartItem item = new CartItem(product, (int)qty);
                //lay gio hang tu session
                Cart cart = (Cart)Session["Cart"];
                if(cart == null)
                {
                    Customer customer = new Customer("nguyen van an", "0987654321", "so 69 pho 420");
                    cart = new Cart();
                    cart.Customer = customer;
                }
                cart.AddToCart(item);
                Session["cart"] = cart; //them session
            }catch (Exception e)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Cart"); 
        }
        public ActionResult Cart()
        {
            return View();
        }

        public ActionResult RemoveItem(int? id)
        {
            try
            {
                Cart cart = (Cart)Session["Cart"];
                if (cart == null)
                {
                    return HttpNotFound();
                }
                cart.RemoveItem((int)id);
                Session["cart"] = cart; //them session
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Cart");
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "Name");
            ViewBag.BrandID = new SelectList(db.Brands, "Id", "Name");
            return View();
        }
        
        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Image,Price,Description,CategoryID,BrandID")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "Name", product.CategoryID);
            ViewBag.BrandID = new SelectList(db.Brands, "Id", "Name", product.BrandID);
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
            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "Name", product.CategoryID);
            ViewBag.BrandID = new SelectList(db.Brands, "Id", "Name", product.BrandID);
            return View(product);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Image,Price,Description,CategoryID")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "Name", product.CategoryID);
            ViewBag.BrandID = new SelectList(db.Brands, "Id", "Name", product.BrandID);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckOut(Order order)
        {
            if(ModelState.IsValid)
            {
                var cart = (Cart)Session["cart"];
                order.GrandTotal = cart.GrandTotal;
                order.CreatedAt = DateTime.Now;
                order.Status = 1;
                db.Orders.Add(order);
                db.SaveChanges();
                foreach(var item in cart.CartItems)
                {
                    OrderItem orderItem = new OrderItem() { OrderID = order.Id, ProductID = item.Product.Id, Qty = item.Quantity, Price = item.Product.Price };
                    db.OrderItems.Add(orderItem);
                }
                db.SaveChanges();
                Session["cart"] = null;
            }
            return RedirectToAction("checkoutsuccess");
        }
        public string CheckOutSuccess()
        {
            return "tao don hang thanh cong";
        }
    }
}
