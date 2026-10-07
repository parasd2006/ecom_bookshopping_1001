
using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using ecom_bookshopping_1001.Models.ViewModels;
using ecom_bookshopping_1001.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ecom_bookshopping_1001.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        public HomeController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims!=null)
            {
                var count = _unitofWork.shoppingCart.GetAll(sc => sc.ApplicationUserId ==
                claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }

           return View(_unitofWork.product.GetAll());

        }
        [HttpGet]
        public IActionResult Search(string searchType, string searchText)
        {
            var products = _unitofWork.product.GetAll();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (searchType == "title")
                {
                    products = products.Where(p =>
                        p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                }
                else if (searchType == "author")
                {
                    products = products.Where(p =>
                        p.Author.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    products = products.Where(p =>
                        p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        p.Author.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                }
            }

            return PartialView("Productlist", products);
        }

        public IActionResult Details(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitofWork.shoppingCart.GetAll(sc => sc.ApplicationUserId ==
                claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            var productInDb = _unitofWork.product.FirstOrDefault(p => p.Id == id, includeProperties: "CopyType,Category");
            if (productInDb == null) return NotFound();
            var shoppingCart = new ShoppingCart()
            {
                product = productInDb,
                ProductId = id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if(ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if(claims == null) return NotFound();
                shoppingCart.ApplicationUserId = claims.Value;
                var shoppingcartInDb = _unitofWork.shoppingCart.FirstOrDefault(sc =>
                sc.ApplicationUserId == claims.Value && 
                sc.ProductId == shoppingCart.ProductId);

                if (shoppingcartInDb == null)
                    _unitofWork.shoppingCart.Add(shoppingCart);
                else
                    shoppingcartInDb.Count += shoppingCart.Count;
                _unitofWork.save();
                return RedirectToAction("Index");
            }else
            {
                var productInDb=_unitofWork.product.FirstOrDefault(p=>
                p.Id == shoppingCart.ProductId, includeProperties: "Category,CopyType");
                if (productInDb == null) return NotFound();
                var shoppingcartEdit = new ShoppingCart();
                return View(shoppingCart);
            }
            return View(shoppingCart);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
