using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using ecom_bookshopping_1001.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1001.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        public CoverTypeController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

       

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            CopyType coverType = new CopyType();
            if (id == null) return View(coverType);
            coverType = _unitofWork.covertype.Get(id.GetValueOrDefault());
            if (coverType == null) return NotFound();
                return View(coverType);

        }
        [HttpPost]
        public IActionResult Upsert(CopyType coverType)
        {
            if (coverType == null) return BadRequest();
            if (!ModelState.IsValid) return NotFound();
            if (coverType.Id != 0)
                _unitofWork.covertype.Add(coverType);
            else
                _unitofWork.covertype.Update(coverType);
            _unitofWork.save();
            return RedirectToAction("Index");
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofWork.covertype.GetAll() });
            }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var covertypelist = _unitofWork.covertype.Get(id);
            if (covertypelist == null)
                return Json(new { success = false, message = "Unable to Delete data...." });
            _unitofWork.covertype.Remove(covertypelist);
            _unitofWork.save();
            return Json(new { success = true, message = "Data deleted successfully..." });



        }
        #endregion
    }

}

