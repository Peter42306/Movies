using Movies.Models;

namespace Movies.Repository
{
    public interface IRepository
    {
        Task<List<Movie>> GetAll();
        Task<Movie> GetById(int id);
        Task Create(Movie movie);
        void Update(Movie movie);
        Task Delete(int id);
        Task Save();
    }
}
