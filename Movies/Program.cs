using Microsoft.EntityFrameworkCore;
using Movies.Models;
using Movies.Repository;

var builder = WebApplication.CreateBuilder(args);

// �������� ������ ����������� �� ����� ������������
string? connection = builder.Configuration.GetConnectionString("DefaultConnection");

// ��������� �������� ApplicationContext � �������� ������� � ����������
//builder.Services.AddDbContext<StudentContext>(options => options.UseSqlServer(connection));
builder.Services.AddDbContext<MovieContext>(options => options.UseSqlServer(connection));

// ��������� ������� MVC
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IRepository<Movie>,MovieRepository>();

var app = builder.Build();

app.UseStaticFiles(); // ������������ ������� � ������ � ����� wwwroot

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movie}/{action=Index}/{id?}");

app.Run();
