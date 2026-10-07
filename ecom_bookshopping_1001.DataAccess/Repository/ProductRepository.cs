using ecom_bookshopping_1001.DataAccess.Data;
using ecom_bookshopping_1001.DataAccess.Repository.IRepository;
using ecom_bookshopping_1001.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ecom_bookshopping_1001.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
