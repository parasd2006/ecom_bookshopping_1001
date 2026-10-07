using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1001.DataAccess.Repository.IRepository
{
    public interface IUnitofWork
    {
         ICategoryRepository category { get; }
        ICovertypeRepository covertype { get; }
        IProductRepository product { get; }
        ICompanyRepository company { get; }
        IApplicationRepository ApplicationUser { get; }
        IShoppingCartRepository shoppingCart { get; }
        IOrderDetailRepository orderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }



        void save();
    }
}
