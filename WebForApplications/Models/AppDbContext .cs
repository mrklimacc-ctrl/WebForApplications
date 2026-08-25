using Microsoft.EntityFrameworkCore;

namespace WebForApplications.Models
{
    public class AppDbContext : DbContext
    {
        // Обязательный конструктор для передачи настроек подключения
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<ApplicationStatus> ApplicationStatuses => Set<ApplicationStatus>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Автоматическое заполнение справочника статусов при миграции
            modelBuilder.Entity<ApplicationStatus>().HasData(
                new ApplicationStatus { Id = 1, Name = "Новая" },
                new ApplicationStatus { Id = 2, Name = "В работе" },
                new ApplicationStatus { Id = 3, Name = "Выполнена" }
            );

            
            modelBuilder.Entity<Application>(entity =>
            {
                // Связь для автора заявки
                entity.HasOne(a => a.Author)
                .WithMany()
                .HasForeignKey(a => a.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

                // Связь для исполнителя заявки
                entity.HasOne(a => a.Executor)
                .WithMany()
                .HasForeignKey(a => a.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);

                // Связь для статуса заявки
                entity.HasOne(a => a.Status)
                .WithMany()
                .HasForeignKey(a => a.StatusId)
                .IsRequired(); 
            });

        }
    }
}
