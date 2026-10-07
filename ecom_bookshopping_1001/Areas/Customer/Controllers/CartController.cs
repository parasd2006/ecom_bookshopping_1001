using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using ecom_bookshopping_1001.Models.ViewModels;
using ecom_bookshopping_1001.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Security.Claims;

namespace ecom_bookshopping_1001.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IConfiguration _config;
        public CartController(IUnitofWork unitofWork,IConfiguration config)
        {
            _unitofWork = unitofWork;
            _config = config;
        }
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims==null)
            {
                shoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>()
                };
                return View(shoppingCartVM);
            }
            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofWork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value,
                includeProperties: "product"),
                orderHeader = new OrderHeader()
            };
            shoppingCartVM.orderHeader.OrderTotal = 0;
            shoppingCartVM.orderHeader.ApplicationUser = _unitofWork.ApplicationUser.FirstOrDefault(
                au => au.Id == claims.Value);
            foreach(var list in shoppingCartVM.ListCart)
            {
                list.price = SD.GetPriceBasedOnQuantity(list.Count, list.product.Price, list.product.Price50, list.product.Price100);
                shoppingCartVM.orderHeader.OrderTotal += (list.Count * list.price);

                if(list.product.Description.Length>100)
                {
                    list.product.Description = list.product.Description.Substring(0, 99) + "...";
                }
            }
            return View(shoppingCartVM);
        }
        public IActionResult plus(int id)
        {
            var cart = _unitofWork.shoppingCart.Get(id);
            cart.Count += 1;
            _unitofWork.save();
            return RedirectToAction("Index");

        }
        public IActionResult minus(int id)
        {
            var cart = _unitofWork.shoppingCart.Get(id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitofWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var cart = _unitofWork.shoppingCart.Get(id);
            _unitofWork.shoppingCart.Remove(cart);
            _unitofWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofWork.shoppingCart.GetAll(
                    sc => sc.ApplicationUserId == claims.Value, includeProperties: "product"),
                orderHeader = new OrderHeader()

            };
            shoppingCartVM.orderHeader.ApplicationUser = _unitofWork.ApplicationUser.FirstOrDefault(
                au => au.Id == claims.Value);
            foreach(var list in shoppingCartVM.ListCart)
            {
                list.price=SD.GetPriceBasedOnQuantity(list.Count,list.product.Price, list.product.Price50,
                    list.product.Price100);
                shoppingCartVM.orderHeader.OrderTotal += (list.price * list.Count);
                if(list.product.Description.Length>100)
                {
                    list.product.Description = list.product.Description.Substring(0, 99) + "...";
                }
            }
            shoppingCartVM.orderHeader.Name = shoppingCartVM.orderHeader.ApplicationUser.Name;
            shoppingCartVM.orderHeader.StreetAddress = shoppingCartVM.orderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.orderHeader.City = shoppingCartVM.orderHeader.ApplicationUser.City;
            shoppingCartVM.orderHeader.State = shoppingCartVM.orderHeader.ApplicationUser.State;
            shoppingCartVM.orderHeader.PostalCode = shoppingCartVM.orderHeader.ApplicationUser.PostalCode;
            shoppingCartVM.orderHeader.PhoneNumber = shoppingCartVM.orderHeader.ApplicationUser.PhoneNumber;

            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claims == null) return NotFound();

            shoppingCartVM.ListCart = _unitofWork.shoppingCart.GetAll(
                sc => sc.ApplicationUserId == claims.Value, includeProperties: "product");

            shoppingCartVM.orderHeader.ApplicationUser=_unitofWork.ApplicationUser.FirstOrDefault
                (au=>au.Id== claims.Value) ;

            shoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusPending;
            shoppingCartVM.orderHeader.PaymentStatus = SD.OrderStatusPending;
            shoppingCartVM.orderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.orderHeader.ApplicationUserId = claims.Value;

            _unitofWork.OrderHeader.Add(shoppingCartVM.orderHeader);
            _unitofWork.save();

            foreach(var list in shoppingCartVM.ListCart)
            {
                list.price=SD.GetPriceBasedOnQuantity(list.Count,list.product.Price,
                    list.product.Price50,list.product.Price100);

                shoppingCartVM.orderHeader.OrderTotal += (list.price * list.Count);
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = shoppingCartVM.orderHeader.Id,
                    ProductId=list.ProductId,
                    Price=list.price,
                    Count=list.Count

                };
                _unitofWork.orderDetail.Add(orderDetail);
                _unitofWork.save();

            }

           // ------------------------------------------------------------------
            //  string keyId = _config["Razorpay:KeyId"];
            //string keySecret = _config["Razorpay:KeySecret"];
            //RazorpayClient client = new RazorpayClient(keyId, keySecret);

            //var domain = $"{Request.Scheme}://{Request.Host}";

           
            //Dictionary<string, object> paymentLinkRequest = new Dictionary<string, object>
            //{
            //    { "amount", (int)(shoppingCartVM.orderHeader.OrderTotal * 100) },
            //    { "currency", "INR" },
            //    { "description", $"Payment for Order #{shoppingCartVM.orderHeader.Id}" },
            //    { "customer", new Dictionary<string, object>
            //        {
            //            { "name", shoppingCartVM.orderHeader.Name ?? "Customer" },
            //            { "contact", shoppingCartVM.orderHeader.PhoneNumber ?? "9999999999" }
            //        }
            //    },
            //     //Payment hone ke baad Razorpay is URL par redirect karega:
            //    { "callback_url", $"{domain}/Customer/Cart/OrderConfirmation?id={shoppingCartVM.orderHeader.Id}" },
            //    { "callback_method", "get" }
            //};

            //PaymentLink paymentLink = client.PaymentLink.Create(paymentLinkRequest);
            //string paymentUrl = paymentLink["short_url"].ToString();

            //// Seedha Razorpay ke hosted payment page par bhej dein
            //return Redirect(paymentUrl);
            
            //----------------------------------------------------------------------------------------

            //remove from shoppingcart model
            _unitofWork.shoppingCart.RemoveRange(shoppingCartVM.ListCart);
            _unitofWork.save();
            //session couunt
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            return RedirectToAction("orderConfirmation", "Cart",
                new { id = shoppingCartVM.orderHeader.Id });

           
        }
        public IActionResult orderConfirmation(int id)
        {
            return View();
        }
        //public IActionResult orderConfirmation(int id, string razorpay_payment_id = null, string razorpay_payment_link_status = null)
        //{
        //    var orderHeader = _unitofWork.OrderHeader.FirstOrDefault(u => u.Id == id);

        //    if (orderHeader != null)
        //    {
        //        // Razorpay payment success hone par status update karein
        //        if (razorpay_payment_link_status == "paid" || !string.IsNullOrEmpty(razorpay_payment_id))
        //        {
        //            orderHeader.TransactionId = razorpay_payment_id;
        //            orderHeader.PaymentStatus = SD.PaymentStatusApproved; // Aapke SD ka constant
        //            orderHeader.OrderStatus = SD.OrderStatusApproved;     // Aapke SD ka constant
        //            orderHeader.PaymentDate = DateTime.Now;
        //            _unitofWork.save();

        //            // Cart delete karein
        //            var cartList = _unitofWork.shoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId);
        //            _unitofWork.shoppingCart.RemoveRange(cartList);
        //            _unitofWork.save();

        //            // Session cart count 0 karein
        //            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
        //        }
        //    }

        //    return View(id);
        //}
    }
}
