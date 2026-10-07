using ecom_bookshopping_1001.DataAccess.Data;
using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1001.DataAccess.Repository
{
   
    public class UnitofWork : IUnitofWork
    {
        private readonly ApplicationDbContext _context;
        public UnitofWork(ApplicationDbContext context)
        {
            _context = context;
            category = new CategoryRepository(context);
            covertype = new CoverTypeRepository(context);
            product = new ProductRepository(context);
            company = new CompanyRepository(context);
            ApplicationUser = new ApplicationRepository(context);
            shoppingCart = new ShoppingCartRepository(context);
            orderDetail = new OrderDetailRepository(context);
            OrderHeader = new OrderHeaderRepository(context);

           

        }
        public ICategoryRepository category {private  set; get; }

        public ICovertypeRepository covertype { private set; get; }

        public IProductRepository product { private set; get; }
        public ICompanyRepository company { private set; get; }

        public IApplicationRepository ApplicationUser { private set; get; }

        public IShoppingCartRepository shoppingCart { private set; get; }

        public IOrderDetailRepository orderDetail { private set; get; }

        public IOrderHeaderRepository OrderHeader { private set; get; }

        public void save()
        {
            _context.SaveChanges();
        }
    }
}
