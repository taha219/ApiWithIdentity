using System.Linq.Expressions;
using Identity_Roles_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity_Roles_API.Repos
{
    public class Base<T> :IBase<T> where T :class
    {
        private readonly AppDbContext _context;

        public Base(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
         return await _context.Set<T>().ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAllAsyncWith(string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes?.Any() == true)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return  await query.ToListAsync(); 
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
       
       public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        { 
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.SingleOrDefaultAsync(criteria);
        }
        public async Task<T> InsertItemAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }
        public async Task<T> UpdateItemAsync(T entity)
        {
             _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task DeleteItemAsync(int id) 
        {
            var prod = await _context.Set<T>().FindAsync(id);
            if (prod==null)
            {
                throw new Exception("item not found");
            }
            _context.Set<T>().Remove(prod);
            await _context.SaveChangesAsync();
        }
        

    }
}
