using ecom_bookshopping_1001.DataAccess.Data;
using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1001.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context= context;  
        }
    }
}
