using Mapster;
using Microsoft.EntityFrameworkCore;
using WebForApplications.DTOs;
using WebForApplications.Models;
using WebForApplications.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем репозиторий и сервис отчетов в контейнере внедрения зависимостей
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Подключаем DbContext (параметры для DefaultConnection в закрытом файле)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Подключаем контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Для корректной обработки схем
TypeAdapterConfig<KeyValuePair<string, int>, StatusDict>.NewConfig()
    .Map(dest => dest.StatusName, src => src.Key)
    .Map(dest => dest.NumberOfApplications, src => src.Value);
TypeAdapterConfig<ExecutorAppCount, TopExecutorDto>.NewConfig()
                .Map(dest => dest.Employee, src => src.Executor);


var app = builder.Build();

// настройка миграций
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Наполняем базу фейковыми данными
    DbSeeder.SeedData(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
