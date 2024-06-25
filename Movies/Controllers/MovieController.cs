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
        IWebHostEnvironment _webHostEnvironment;

        IRepository _repository;


        //// Внедряем ссылки через конструктор
        //public MovieController(MovieContext movieContext, IWebHostEnvironment webHostEnvironment)
        //{
        //    _movieContext = movieContext;
        //    _webHostEnvironment = webHostEnvironment;
        //}

        public MovieController(IRepository repository, IWebHostEnvironment webHostEnvironment)
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

						//await uploadedFile.CopyToAsync(fileStream);						
                    }

                    // Устанавливаем путь к изображению в объекте фильма
                    movie.PosterPath = "/Image/" + newFileNameGenerated;
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


        //[HttpPost]
        //public async Task<IActionResult> AddFile(IFormFile uploadedFile, Movie movie)
        //{
        //    if (uploadedFile != null && movie != null)
        //    {
        //        string fileName = Path.GetFileName(uploadedFile.FileName);
        //        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", fileName);
        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await uploadedFile.CopyToAsync(fileStream);
        //        }
        //        movie.PosterPath = "/Image/" + fileName;

        //        _movieContext.Movies.Add(movie);
        //        await _movieContext.SaveChangesAsync();

        //        //string posterPath = "/Image/" + uploadedFile.FileName;

        //        //using (var fileStream = new FileStream(_webHostEnvironment.WebRootPath + posterPath, FileMode.Create))
        //        //{
        //        //    await uploadedFile.CopyToAsync(fileStream);
        //        //}

        //        //Movie posterFile = new Movie { Title = movie, PosterPath = posterPath };
        //        //_movieContext.Movies.Add(posterFile);
        //        //_movieContext.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}



        //// Post запрос на Create
        //[HttpPost] // получение данных от клиента, при нажатии СОХРАНИТЬ данные сохраняются в БД
        //[ValidateAntiForgeryToken] // проверка данных перед тем как они отправятся в базу данных

        //// Bind - инициализация полей объекта, происходит напрямую через форму html
        //public async Task<IActionResult> Create([Bind("Id,Title,Director,Genre,ReleaseYear,PosterPath,Description")] Movie movie, IFormFile posterFile)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (posterFile != null && posterFile.Length > 0)
        //        {
        //            string fileName = Path.GetFileName(posterFile.FileName);
        //            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", fileName);

        //            using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await posterFile.CopyToAsync(fileStream);
        //            }
        //        }

        //        _movieContext.Add(movie);
        //        await _movieContext.SaveChangesAsync(); // синхронизация с БД
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(movie);
        //}

        // !!! c этим методом сохраняется и отображается новый фильм, но нет постера !!!
        // Bind - инициализация полей объекта, происходит напрямую через форму html
        //[HttpPost]
        //public async Task<IActionResult> Create([Bind("Id,Title,Director,Genre,ReleaseYear,PosterPath,Description")] Movie movie)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _movieContext.Add(movie);
        //        await _movieContext.SaveChangesAsync(); // синхронизация с БД
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(movie);
        //}


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


        // POST запрос для редактирования фильма
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, [Bind("Id,Title,Director,Genre,ReleaseYear,Description")] Movie movie, IFormFile uploadedFile)
        {
            if (Id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");
                    string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
                    string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
						// Load the image
						using (var image=await Image.LoadAsync(uploadedFile.OpenReadStream()))
                        {
                            // Resize the image to a maximum width and height of 800px
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
								Size = new Size(600,600)
								//Size = new Size(image.Width, image.Height)
							}));

                            await image.SaveAsPngAsync(fileStream);
						}

						//await uploadedFile.CopyToAsync(fileStream);
                    }

                    movie.PosterPath = "/Image/" + newFileNameGenerated;                                        
                }                
                    _repository.Update(movie);
                    await _repository.Save();
                    return RedirectToAction("Index", "Movie");
            }
            return View(movie);

            //if (Id != movie.Id)
            //{
            //    return NotFound();
            //}

            //if (ModelState.IsValid)
            //{
            //    if (uploadedFile != null && uploadedFile.Length > 0)
            //    {
            //        string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");
            //        string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
            //        string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

            //        using (var fileStream = new FileStream(filePath, FileMode.Create))
            //        {
            //            await uploadedFile.CopyToAsync(fileStream);
            //        }

            //        movie.PosterPath = "/Image/" + newFileNameGenerated;
            //    }

            //    _movieContext.Update(movie);
            //    await _movieContext.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}
            //return View(movie);
        }


        //     // !!! этот метод по редактированию работает, но частично, загружаются фото только из папки wwwroot
        //     [HttpPost]
        //     [ValidateAntiForgeryToken]
        //     public async Task<IActionResult>Edit(int id, [Bind("Id,Title,Director,Genre,ReleaseYear,PosterPath,Description")] Movie movie)
        //     {
        //         string fullPath = "/Image/" + movie.PosterPath;
        //         movie.PosterPath = fullPath;

        //         if(id != movie.Id)
        //         {
        //             return NotFound();
        //         }

        //         if (ModelState.IsValid)
        //         {
        //             try
        //             {
        //                 _movieContext.Update(movie);
        //                 await _movieContext.SaveChangesAsync();
        //             }
        //             catch (DbUpdateConcurrencyException)
        //             {
        //                 if (!MovieExists(movie.Id))
        //                 {
        //                     return NotFound();
        //                 }
        //                 else
        //                 {
        //                     throw;
        //                 }					
        //	}
        //	return RedirectToAction(nameof(Index));
        //}
        //         return View(movie);
        //     }

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
            List<Movie> list = await _repository.GetAll();
            return (list?.Any(m => m.Id == id)).GetValueOrDefault();

            //return _movieContext.Movies.Any(item => item.Id == id);
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
