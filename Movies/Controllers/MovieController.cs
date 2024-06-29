using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies.Models;
using Movies.Repository;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Movies.Controllers
{
    public class MovieController : Controller
    {
        ///////////////////////////////////////////////////////////////////////////////////////////

        // Объявление контекста базы данных фильмов
        //MovieContext _movieContext;

        // IWebHostEnvironment предоставляет информацию об окружении, в котором запущено приложение
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IRepository<Movie> _repository;


        //// Внедряем ссылки через конструктор
        //public MovieController(MovieContext movieContext, IWebHostEnvironment webHostEnvironment)
        //{
        //    _movieContext = movieContext;
        //    _webHostEnvironment = webHostEnvironment;
        //}

        public MovieController(IRepository<Movie> repository, IWebHostEnvironment webHostEnvironment)
        {
            _repository = repository;
            _webHostEnvironment = webHostEnvironment;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        // GET запрос для отображения всех фильмов в списке
        public async Task<IActionResult> Index()
        {
            var model = await _repository.GetAll();
            return View(model);

            //return View(await _movieContext.Movies.ToArrayAsync());
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        // GET запрос для отображения деталей конкретного выбранного фильма
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || await _repository.GetAll() == null)
            {
                return NotFound();
            }

            var movie = await _repository.GetById((int)id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);

            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var movie = await _movieContext.Movies.FirstOrDefaultAsync(item => item.Id == id);

            //if (movie == null)
            //{
            //    return NotFound();
            //}

            //return View(movie);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        // Get запрос на Create

        /*
        public IActionResult Create()
		{
            return View(_movieContext.Movies.ToList());
		}
        */

        // GET запрос для отображения формы создания нового фильма
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST запрос для создания нового фильма в списке
        // Bind - инициализация полей объекта, происходит напрямую через форму html
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Director,Genre,ReleaseYear,Description")] Movie movie, IFormFile uploadedFile)
        {

            if (ModelState.IsValid)
            {
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    movie.PosterPath=await UploadPicture(uploadedFile);
                }

                // Добавляем фильм в репозиторий
                await _repository.Create(movie);

                // Сохраняем изменения в базе данных
                await _repository.Save();

                // Перенаправляем на страницу списка фильмов
                return RedirectToAction(nameof(Index));
            }

            // Если модель не валидна, возвращаем представление с текущими данными фильма
            return View(movie);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        // GET запрос на отображение формы редактирования фильма
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || await _repository.GetAll() == null)
            {
                return NotFound();
            }

            var movie = await _repository.GetById((int)id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);


            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var movie = await _movieContext.Movies.FindAsync(id);

            //if (movie == null)
            //{
            //    return NotFound();
            //}

            //return View(movie);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, [Bind("Id,Title,Director,Genre,ReleaseYear,PosterPath,Description")] Movie movie, IFormFile? uploadedFile)
        {
            if (Id != movie.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(movie);
            }

            try
            {
                // Fetch the existing movie from the database
                var existingMovie = await _repository.GetById(Id);
                if (existingMovie == null)
                {
                    return NotFound();
                }

                // Update only the properties that are changed
                
                existingMovie.Title = movie.Title;
                existingMovie.Director = movie.Director;
                existingMovie.Genre = movie.Genre;
                existingMovie.ReleaseYear = movie.ReleaseYear;
                existingMovie.Description = movie.Description;
                

                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    // Process the uploaded file and get the new file path
                    existingMovie.PosterPath=await UploadPicture(uploadedFile);
                }

                // Update the movie in the repository
                _repository.Update(existingMovie);

                // Save the changes to the database
                await _repository.Save();

                return RedirectToAction("Index", "Movie");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MovieExists(movie.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            //return View(movie);
        }                

        ///////////////////////////////////////////////////////////////////////////////////////////

        // GET запрос формы для удаления фильма
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || await _repository.GetAll() == null)
            {
                return NotFound();
            }

            //var movie = await  _movieContext.Movies.FirstOrDefaultAsync(item => item.Id == id);
            var movie = await _repository.GetById((int)id);

            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);

            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var movie = await _movieContext.Movies.FirstOrDefaultAsync(item => item.Id == id);

            //if (movie == null)
            //{
            //    return NotFound();
            //}
            //return View(movie);
        }

        // POST запрос для удаления фильма
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // почему пропало подчёркивание когда я написал return RedirectToAction(nameof(Index));
        {
            if (await _repository.GetAll() == null)
            {
                return Problem("Entity set 'MovieContext' is null");
            }

            var movie = await _repository.GetById(id);

            if (movie != null)
            {
                //_movieContext.Movies.Remove(movie);
                _repository.Delete(id);
            }

            //await _movieContext.SaveChangesAsync();
            await _repository.Save();
            return RedirectToAction(nameof(Index));


            //var movie = await _movieContext.Movies.FindAsync(id);

            //if (movie != null)
            //{
            //    _movieContext.Movies.Remove(movie);
            //}

            //await _movieContext.SaveChangesAsync();

            //return RedirectToAction(nameof(Index));

        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        // Вспомогательный метод для проверки существования фильма
        private async Task<bool> MovieExists(int id)
        {
            var movie = await _repository.GetById(id);
            return movie != null;

            //List<Movie> list = await _repository.GetAll();
            //return (list?.Any(m => m.Id == id)).GetValueOrDefault();

            //return _movieContext.Movies.Any(item => item.Id == id);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        // Вспомогательный метод для загрузки картинок, с уменьшением размера фото
        private async Task<string> UploadPicture(IFormFile uploadedFile)
        {
            // Путь к папке, где будут храниться изображения
            string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");

            // Генерируем новое уникальное имя файла для изображения
            string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;

            // Полный путь к файлу на сервере
            string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

            // Сохраняем файл на сервере
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                // Load the image
                using (var image = await Image.LoadAsync(uploadedFile.OpenReadStream()))
                {
                    // Resize the image to a maximum width and height of 800px
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(600, 600)
                        //Size = new Size(image.Width, image.Height)
                    }));

                    await image.SaveAsPngAsync(fileStream);
                }
            }
            // Устанавливаем путь к изображению в объекте фильма
            return "/Image/" + newFileNameGenerated;
        }

        //// Действие (action) для отображения списка фильмов
        //public async Task<IActionResult> Index()
        //{
        //    // Получение списка фильмов из базы данных
        //    IEnumerable<Movie>movies=await Task.Run(()=>_movieContext.Movies);

        //    // Передача списка фильмов в представление через ViewBag
        //    ViewBag.Movies = movies;

        //    // Возвращение представления Index
        //    return View();
        //}
    }
}
