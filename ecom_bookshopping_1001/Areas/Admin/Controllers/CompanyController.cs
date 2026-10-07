using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using ecom_bookshopping_1001.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1001.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        public CompanyController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null) return View(company);
            company = _unitofWork.company.Get(id.GetValueOrDefault());
            if (company == null) return NotFound();
            return View(company);
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (company == null) return BadRequest();
            if (!ModelState.IsValid) return NotFound();
            if (company.Id == 0)
                _unitofWork.company.Add(company);
            else
                _unitofWork.company.Update(company);
            _unitofWork.save();
            return RedirectToAction("Index");

        }
        #region APIs
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofWork.company.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companylist = _unitofWork.company.Get(id);
            if (companylist == null)
                return Json(new { success = false, message = "unable to delete data..." });
            _unitofWork.company.Remove(companylist);
            _unitofWork.save();
            return Json(new { success = true, message = "Data deleted successfully..." });
        }
        #endregion
    }
}
