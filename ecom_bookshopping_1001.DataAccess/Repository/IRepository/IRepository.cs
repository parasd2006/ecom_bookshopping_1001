using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ecom_bookshopping_1001.DataAccess.Repository.IRepository
{
    public  interface IRepository<T> where T: class
        //T sirf kisi class/type ko represent kar sakta hai, value type ko nahi.
        //value jese" IRepository<double> "
    {
        void Add(T entity);
        void Update(T entity);

        T Get(int id);
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null);

        T FirstOrDefault(Expression<Func<T,bool>> filter=null,
            string includeProperties=null);
      
        void Remove(int id);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
