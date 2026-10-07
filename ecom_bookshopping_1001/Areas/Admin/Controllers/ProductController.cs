using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using ecom_bookshopping_1001.Models.ViewModels;
using ecom_bookshopping_1001.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;

namespace ecom_bookshopping_1001.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitofWork _unitofwork;
        public ProductController(IUnitofWork unitofWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = unitofWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                product = new Product(),

                CategoryList = _unitofwork.category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                }),
                CoverTypeList=_unitofwork.covertype.GetAll().Select(ct=> new SelectListItem()
                {
                    Text=ct.Name,
                    Value=ct.Id.ToString()
                })

                
            };
            if (id == null) return View(productVM);
            productVM.product = _unitofwork.product.Get(id.GetValueOrDefault());
            if (productVM.product == null) return NotFound();
            return View(productVM);

        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM)
        {
            if(ModelState.IsValid)//iska mtlb Kya user ne form me jo data diya hai woh model ki validation rules ko satisfy karta hai?
                //agr form fill hai toh true hokr block condition execute hogi
            {
                var webRootPath = _webHostEnvironment.WebRootPath;//WebRootPath tumhe wwwroot folder ka actual computer path deta hai. orr usko hum variable meh store krte hai
                var files = HttpContext.Request.Form.Files;//"Current HTTP request ke form ke andar jo files upload hui hain, unko files variable me rakho."
                if (files.Count()>0)// agr koi image upload nhi hui toh if(0 > 0) yeh block nhi chlega
                                    //orr agr image upload hui if(1 > 0) toh block chlega
                {
                    var fileName = Guid.NewGuid().ToString();//Ye line ek unique random ID generate karti hai.
                    //suppose agr user book.jpg upload krta hai toh hum uski uniquw id upload krege 
                    //kyuki book.jpg name same bhi ho skta hai 
                    var extension = Path.GetExtension(files[0].FileName);
                    //getextension => .jpg, .png etc
                    var uploads = Path.Combine(webRootPath, @"Images\Products");
                    //Path.Combine() dono ko properly combine karega C:\Project\wwwroot\images\products
                    //Path.Combine() path separators ko properly handle karta hai.
                    if (productVM.product.Id!=0)//Id=0 mtlb new product 
                        //Id=5 mtlb update product
                    {
                        var imageExist = _unitofwork.product.Get(productVM.product.Id).Imageurl;
                        //(productVM.product.Id = 5)
                        //_unitofwork.product.Get(5).Imageurl
                        //\images\products\oldbook.jpg
                        productVM.product.Imageurl = imageExist;//"Purani image ka path leke current Product object me rakh do, taaki image ka reference lose na ho."
                    }
                    if(productVM.product.Imageurl!=null)//Kya Product ke paas image ka path hai?
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.product.Imageurl.Trim('\\'));
                        //Path.Combine(C:\Project\wwwroot\images\products\oldbook.jpg)
                        //trim mtlb starting orr ending backslash remove krega
                        //Ab imagePath ke andar actual computer location aa gayi.
                        if (System.IO.File.Exists(imagePath))//yeh check krta hai Kya is location par actual file present hai?
                        {
                            System.IO.File.Delete(imagePath);//Agar file exist karti hai toh ye actual computer se file delete kar dega.
                        }
                    }
                 using (var filestream= new FileStream(Path.Combine(uploads,fileName+ extension),FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }
                    productVM.product.Imageurl = @"\Images\Products\" + fileName + extension;

                }
                else
                {
                    if(productVM.product.Id!=0)
                    {

                        var imageexists = _unitofwork.product.Get(productVM.product.Id).Imageurl;
                        productVM.product.Imageurl = imageexists;
                    }
                }
                if (productVM.product.Id == 0)
                    _unitofwork.product.Add(productVM.product);
                else
                    _unitofwork.product.Update(productVM.product);
                _unitofwork.save();
                return RedirectToAction("Index");


            }
            else
            {
                 productVM = new ProductVM()
                {
                    product = new Product(),

                    CategoryList = _unitofwork.category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _unitofwork.covertype.GetAll().Select(ct => new SelectListItem()
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()
                    })


                };
               if(productVM.product.Id!=0)
                {
                    productVM.product = _unitofwork.product.Get(productVM.product.Id);
                    if (productVM.product == null) return NotFound();
                }
                return View(productVM);
            }
        }
        #region APIs
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofwork.product.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productInDb = _unitofwork.product.Get(id);
            if (productInDb == null)
                return Json(new { success = false, message = "unable to delete data.." });
            //image delete logic
            var webRootPath = _webHostEnvironment.WebRootPath;
            var imagepath = Path.Combine(webRootPath, productInDb.Imageurl.Trim('\\'));
            if(System.IO.File.Exists(imagepath))
            {
                System.IO.File.Delete(imagepath);
            }
            _unitofwork.product.Remove(productInDb);
            _unitofwork.save();
            return Json(new { success = true, message = "Data Delete successfully.." });
        }
        #endregion
    }
}
