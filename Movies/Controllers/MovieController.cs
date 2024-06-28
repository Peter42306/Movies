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
                    existingMovie.PosterPath=await UploadingFile(uploadedFile);
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

        private async Task<string> UploadingFile(IFormFile uploadedFile)
        {
            // Path to the folder where images will be stored
            string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");

            // Generate a new unique file name for the image
            string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;

            // Full path to the file on the server
            string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

            // Save the file on the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                // Load the image
                using (var image = await Image.LoadAsync(uploadedFile.OpenReadStream()))
                {
                    // Resize the image to a maximum width and height of 600px
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(600, 600)
                    }));

                    await image.SaveAsPngAsync(fileStream);
                }
            }

            // Set the path to the image in the movie object
            return "/Image/" + newFileNameGenerated;
        }

        ////работает частично
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int Id, [Bind("Id,Title,Director,Genre,ReleaseYear,PosterPath,Description")] Movie movie, IFormFile? uploadedFile)
        ////public async Task<IActionResult> Edit(int Id, [Bind("Id,Title,Director,Genre,ReleaseYear,Description")] Movie movie)
        //{
        //    //var existingMovieById = await _repository.GetById((int)Id);



        //    //await Console.Out.WriteLineAsync($"!!!!!!!!!!!!!! movie path before movie: {movie.PosterPath}");
        //    //await Console.Out.WriteLineAsync($"!!!!!!!!!!!!!! movie {movie.Id} {movie.Title} {movie.Director} {movie.Description} {movie.PosterPath}");            
        //    //await Console.Out.WriteLineAsync($"!!!!!!!!!!!!!! movie path before existingMovieById {existingMovieById.PosterPath}");
        //    //await Console.Out.WriteLineAsync($"!!!!!!!!!!!!!! movie {existingMovieById.Id} {existingMovieById.Title} {existingMovieById.Director} {existingMovieById.Description} {existingMovieById.PosterPath}");
        //    //Console.WriteLine($"!!!!!!!!!!!!!! movie path before {existingPoster}");
        //    //Console.WriteLine($"!!!!!!!!!!!!!! movie path before {movie.PosterPath}");
        //    //Console.WriteLine($"!!!!!!!!!!!!!! movie path before {uploadedFile.Name}");

        //    if (Id != movie.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View(movie);
        //    }

        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if (uploadedFile != null && uploadedFile.Length > 0)
        //            {

        //                // Путь к папке, где будут храниться изображения
        //                string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");

        //                // Генерируем новое уникальное имя файла для изображения
        //                string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;

        //                // Полный путь к файлу на сервере
        //                string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);


        //                // Сохраняем файл на сервере
        //                using (var fileStream = new FileStream(filePath, FileMode.Create))
        //                {
        //                    // Load the image
        //                    using (var image = await Image.LoadAsync(uploadedFile.OpenReadStream()))
        //                    {
        //                        // Resize the image to a maximum width and height of 800px
        //                        image.Mutate(x => x.Resize(new ResizeOptions
        //                        {
        //                            Mode = ResizeMode.Max,
        //                            Size = new Size(600, 600)
        //                            //Size = new Size(image.Width, image.Height)
        //                        }));

        //                        await image.SaveAsPngAsync(fileStream);
        //                    }

        //                    await uploadedFile.CopyToAsync(fileStream);
        //                }

        //                // Устанавливаем путь к изображению в объекте фильма
        //                movie.PosterPath = "/Image/" + newFileNameGenerated;

        //                //await Console.Out.WriteLineAsync($"!!!!!!!!!!!!!! movie path before in if {movie.PosterPath}");
        //                //await Console.Out.WriteLineAsync($"movie {movie.Id} {movie.Title} {movie.Director} {movie.Description} {movie.PosterPath}");
        //            }
        //            else
        //            {


        //                // Если изображение не было загружено, сохраняем существующий путь к изображению
        //                var existingMovie = await _repository.GetById(Id);

        //                movie.PosterPath = existingMovie.PosterPath;




        //                //movie.PosterPath = "/Image/" + movie.PosterPath;

        //                //movie.PosterPath = "/Image/" + existingMovieById.PosterPath;
        //                //await Console.Out.WriteLineAsync($"!!!!!!!!!!!!!! movie path before in else {movie.PosterPath}");
        //                //await Console.Out.WriteLineAsync($"movie {movie.Id} {movie.Title} {movie.Director} {movie.Description} {movie.PosterPath}");
        //            }

        //            // Добавляем фильм в репозиторий
        //            _repository.Update(movie);

        //            // Сохраняем изменения в базе данных
        //            await _repository.Save();

        //            return RedirectToAction("Index", "Movie");

        //        }

        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!await MovieExists(movie.Id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return View(movie);
        //}








        //if (ModelState.IsValid)
        //{
        //    if (uploadedFile != null && uploadedFile.Length > 0)
        //    {


        //        // Путь к папке, где будут храниться изображения
        //        string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");

        //        // Генерируем новое уникальное имя файла для изображения
        //        string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;

        //        // Полный путь к файлу на сервере
        //        string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);


        //        // Сохраняем файл на сервере
        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            // Load the image
        //            using (var image = await Image.LoadAsync(uploadedFile.OpenReadStream()))
        //            {
        //                // Resize the image to a maximum width and height of 800px
        //                image.Mutate(x => x.Resize(new ResizeOptions
        //                {
        //                    Mode = ResizeMode.Max,
        //                    Size = new Size(600, 600)
        //                    //Size = new Size(image.Width, image.Height)
        //                }));

        //                await image.SaveAsPngAsync(fileStream);
        //            }

        //            //await uploadedFile.CopyToAsync(fileStream);						
        //        }

        //        // Устанавливаем путь к изображению в объекте фильма
        //        movie.PosterPath = "/Image/" + newFileNameGenerated;
        //    }

        //    // Добавляем фильм в репозиторий
        //    await _repository.Create(movie);

        //    // Сохраняем изменения в базе данных
        //    await _repository.Save();

        //    // Перенаправляем на страницу списка фильмов
        //    return RedirectToAction(nameof(Index));
        //}

        //// Если модель не валидна, возвращаем представление с текущими данными фильма
        //return View(movie);



        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        ////public async Task<IActionResult> Edit(int Id, [Bind("Id,Title,Director,Genre,ReleaseYear,Description")] Movie movie, IFormFile uploadedFile)
        ////{
        ////    if (Id != movie.Id)
        ////    {
        ////        return NotFound();
        ////    }

        ////    if (!ModelState.IsValid)
        ////    {
        ////        // Log the errors
        ////        foreach (var state in ModelState)
        ////        {
        ////            var key = state.Key;
        ////            var errors = state.Value.Errors;
        ////            foreach (var error in errors)
        ////            {
        ////                Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
        ////            }
        ////        }

        ////        return View(movie);
        ////    }

        ////    try
        ////    {
        ////        if (uploadedFile != null && uploadedFile.Length > 0)
        ////        {
        ////            string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");
        ////            string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
        ////            string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

        ////            using (var imageStream = uploadedFile.OpenReadStream())
        ////            using (var image = await Image.LoadAsync(imageStream))
        ////            {
        ////                image.Mutate(x => x.Resize(new ResizeOptions
        ////                {
        ////                    Mode = ResizeMode.Max,
        ////                    Size = new Size(800, 800)
        ////                }));

        ////                await image.SaveAsPngAsync(filePath);
        ////            }

        ////            movie.PosterPath = "/Image/" + newFileNameGenerated;
        ////        }
        ////        else
        ////        {
        ////            var existingMovie = await _repository.GetById(Id);
        ////            if (existingMovie != null)
        ////            {
        ////                movie.PosterPath = existingMovie.PosterPath;
        ////            }
        ////        }

        ////        _repository.Update(movie);
        ////        await _repository.Save();
        ////        return RedirectToAction(nameof(Index));
        ////    }
        ////    catch (DbUpdateConcurrencyException)
        ////    {
        ////        if (!await MovieExists(movie.Id))
        ////        {
        ////            return NotFound();
        ////        }
        ////        else
        ////        {
        ////            throw;
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        ModelState.AddModelError("", "Ошибка при сохранении изменений: " + ex.Message);
        ////    }

        ////    return View(movie);
        ////}

        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        ////public async Task<IActionResult> Edit(int Id, [Bind("Id,Title,Director,Genre,ReleaseYear,Description")] Movie movie, IFormFile uploadedFile)
        ////{
        ////    if (Id != movie.Id)
        ////    {
        ////        return NotFound();
        ////    }

        ////    if (ModelState.IsValid)
        ////    {
        ////        if (uploadedFile != null && uploadedFile.Length > 0)
        ////        {
        ////            string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");
        ////            string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
        ////            string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

        ////            using (var imageStream = uploadedFile.OpenReadStream())
        ////            using (var image = await Image.LoadAsync(imageStream))
        ////            {
        ////                // Resize the image to a maximum width and height of 800px, maintaining the aspect ratio
        ////                image.Mutate(x => x.Resize(new ResizeOptions
        ////                {
        ////                    Mode = ResizeMode.Max,
        ////                    Size = new Size(800, 800)
        ////                }));

        ////                // Save the resized image as PNG
        ////                await image.SaveAsPngAsync(filePath);
        ////            }

        ////            movie.PosterPath = "/Image/" + newFileNameGenerated;                    
        ////        }

        ////            _repository.Update(movie);
        ////            await _repository.Save();
        ////            return RedirectToAction(nameof(Index));


        ////        //else
        ////        //{
        ////        //    // Keep the existing PosterPath if no new file is uploaded
        ////        //    var existingMovie = await _repository.GetById(Id);
        ////        //    if (existingMovie != null)
        ////        //    {
        ////        //        movie.PosterPath = existingMovie.PosterPath;
        ////        //    }
        ////        //}




        ////    }            
        ////        return View(movie);          

        ////}


        ////// POST запрос для редактирования фильма
        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        ////public async Task<IActionResult> Edit(int Id, [Bind("Id,Title,Director,Genre,ReleaseYear,Description")] Movie movie, IFormFile uploadedFile)
        ////{
        ////    if (Id != movie.Id)
        ////    {
        ////        return NotFound();
        ////    }

        ////    if (ModelState.IsValid)
        ////    {
        ////        try
        ////        {
        ////            if (uploadedFile != null && uploadedFile.Length > 0)
        ////            {
        ////                string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");
        ////                string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
        ////                string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

        ////                using (var imageStream = uploadedFile.OpenReadStream())
        ////                using (var image = await Image.LoadAsync(imageStream))
        ////                {
        ////                    // Resize the image to a maximum width and height of 800px, maintaining the aspect ratio
        ////                    image.Mutate(x => x.Resize(new ResizeOptions
        ////                    {
        ////                        Mode = ResizeMode.Max,
        ////                        Size = new Size(800, 800)
        ////                    }));

        ////                    // Save the resized image as PNG
        ////                    await image.SaveAsPngAsync(filePath);
        ////                }

        ////                movie.PosterPath = "/Image/" + newFileNameGenerated;
        ////            }
        ////            else
        ////            {
        ////                // Keep the existing PosterPath if no new file is uploaded
        ////                var existingMovie = await _repository.GetById(Id);
        ////                if (existingMovie != null)
        ////                {
        ////                    movie.PosterPath = existingMovie.PosterPath;
        ////                }
        ////            }

        ////            _repository.Update(movie);
        ////            await _repository.Save();
        ////            return RedirectToAction(nameof(Index));
        ////        }
        ////        catch (Exception ex)
        ////        {

        ////            ModelState.AddModelError("", "Ошибка при сохранении изменений: " + ex.Message);
        ////        }

        ////    }
        ////    return View(movie);

        ////    //if (Id != movie.Id)
        ////    //{
        ////    //    return NotFound();
        ////    //}

        ////    //if (ModelState.IsValid)
        ////    //{
        ////    //    if (uploadedFile != null && uploadedFile.Length > 0)
        ////    //    {
        ////    //        string uploadedFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Image");
        ////    //        string newFileNameGenerated = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
        ////    //        string filePath = Path.Combine(uploadedFolder, newFileNameGenerated);

        ////    //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        ////    //        {
        ////    //            await uploadedFile.CopyToAsync(fileStream);
        ////    //        }

        ////    //        movie.PosterPath = "/Image/" + newFileNameGenerated;
        ////    //    }

        ////    //    _movieContext.Update(movie);
        ////    //    await _movieContext.SaveChangesAsync();
        ////    //    return RedirectToAction(nameof(Index));
        ////    //}
        ////    //return View(movie);
        ////}


        ////     // !!! этот метод по редактированию работает, но частично, загружаются фото только из папки wwwroot
        ////     [HttpPost]
        ////     [ValidateAntiForgeryToken]
        ////     public async Task<IActionResult>Edit(int id, [Bind("Id,Title,Director,Genre,ReleaseYear,PosterPath,Description")] Movie movie)
        ////     {
        ////         string fullPath = "/Image/" + movie.PosterPath;
        ////         movie.PosterPath = fullPath;

        ////         if(id != movie.Id)
        ////         {
        ////             return NotFound();
        ////         }

        ////         if (ModelState.IsValid)
        ////         {
        ////             try
        ////             {
        ////                 _movieContext.Update(movie);
        ////                 await _movieContext.SaveChangesAsync();
        ////             }
        ////             catch (DbUpdateConcurrencyException)
        ////             {
        ////                 if (!MovieExists(movie.Id))
        ////                 {
        ////                     return NotFound();
        ////                 }
        ////                 else
        ////                 {
        ////                     throw;
        ////                 }					
        ////	}
        ////	return RedirectToAction(nameof(Index));
        ////}
        ////         return View(movie);
        ////     }

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
