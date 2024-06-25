using Microsoft.EntityFrameworkCore;
using Movies.Models;

namespace Movies.Repository
{
    public class MovieRepository:IRepository
    {
        private readonly MovieContext _context;

        public MovieRepository(MovieContext context)
        {
            _context = context;
        }

        public async Task<List<Movie>> GetAll()
        {
            return await _context.Movies.ToListAsync();
        }

        public async Task<Movie> GetById(int movieId)
        {
            return await _context.Movies.FindAsync(movieId);
        }

        public async Task Create(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
        }

        public void Update(Movie movie)
        {
            _context.Entry(movie).State = EntityState.Modified;
        }

        public async Task Delete(int movieId)
        {
            Movie? movie = await _context.Movies.FindAsync(movieId);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
