using ecom_bookshopping_1001.DataAccess.Data;
using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using ecom_bookshopping_1001.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecom_bookshopping_1001.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        private readonly ApplicationDbContext _context;
        public UserController(IUnitofWork unitofWork, ApplicationDbContext context)
        {
            _unitofWork = unitofWork;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        public IActionResult GetAll()
        {
            var userlist = _context.ApplicationUsers.ToList();//AspNetUser
            var rolelist = _context.Roles.ToList();//aspnetroles
            var userRoles = _context.UserRoles.ToList();//aspnetuserroles


            foreach(var user in userlist)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = rolelist.FirstOrDefault(r => r.Id == roleId).Name;
                if(user.ComapnyId==null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
                if(user.ComapnyId !=null)
                {
                    user.Company = new Company()
                    {
                        Name = _unitofWork.company.Get(Convert.ToInt32(user.ComapnyId)).Name
                    };
                }
               
            }
            //Remove admin Role User
            var adminUser = userlist.FirstOrDefault(u => u.Role == SD.Role_Admin);
            userlist.Remove(adminUser);
            return Json(new { data = userlist }); 
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            bool isLocked = false;
            var userindb = _unitofWork.ApplicationUser.FirstOrDefault(u => u.Id == id);
            if(userindb==null)
            {
                return Json(new
                {
                    success = false,
                    message = "Something went wrong while lock and unlock user"
                });
            }
            if (userindb != null && userindb.LockoutEnd>DateTime.Now)
            {
                userindb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }
            else
            {
                userindb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = isLocked == true ? "User Successfully Locked" : "User Successfully Unlocked" });
        }
        #endregion
    }
}
