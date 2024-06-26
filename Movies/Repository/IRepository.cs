using Movies.Models;

namespace Movies.Repository
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAll();
        Task<T> GetById(int id);
        Task Create(T entity);
        void Update(T entity);
        Task Delete(int id);
        Task Save();
    }
}
