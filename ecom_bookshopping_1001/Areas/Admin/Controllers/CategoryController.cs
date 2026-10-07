using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using ecom_bookshopping_1001.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1001.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        public CategoryController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null) return View(category);
            category = _unitofWork.category.Get(id.GetValueOrDefault());
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if (category == null) return BadRequest();
            if (!ModelState.IsValid) return NotFound();
            if (category.Id == 0)
                _unitofWork.category.Add(category);
            else
                _unitofWork.category.Update(category);
            _unitofWork.save();
            return RedirectToAction("Index");

        }
        #region APIs
        //Iska database ya API logic se koi direct relation nahi hai.
        [HttpGet]
        public IActionResult GetAll()
        {
            var category = _unitofWork.category.GetAll();
            return Json(new { data = category });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var category = _unitofWork.category.Get(id);
            if (category == null)
                return Json(new { success = false, message = "unable to delete data" });
            _unitofWork.category.Remove(category);
            _unitofWork.save();
            return Json(new { success = true, message = "successfully delete data..." });
        }

        #endregion
    }
}
