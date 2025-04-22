using System.Linq.Expressions;

namespace Identity_Roles_API.Repos
{
    public interface IBase<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsyncWith(string[] includes = null);
        Task<T> GetByIdAsync(int id);
        Task<T> FindAsync(Expression<Func<T,bool>> criteria, string[] includes =null);
        Task<T> InsertItemAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task<T> UpdateItemAsync(T entity);
         Task DeleteItemAsync(int id);
    }
}
