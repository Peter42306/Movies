namespace Movies.Models
{
    public class Movie
    {
        // Id фильма в БД
        public int Id { get; set; }

        // Название фильма
        public string? Title { get; set; }

        // Режиссер фильма
        public string? Director { get; set; }

        // Жанр фильма
        public string? Genre { get; set; }

        // Год выпуска фильма
        public int ReleaseYear { get; set; }

        // Путь к постеру фильма
        public string? PosterPath { get; set; }

        // Описание фильма
        public string? Description { get; set; }
    }
}
