using Microsoft.EntityFrameworkCore;
using WebForApplications.Models;
using Xunit;

[Collection("PostgresCollection")]
public abstract class TestBase : IDisposable
{
    protected readonly AppDbContext Context;

    protected TestBase(PostgresFixture fixture)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        Context = new AppDbContext(options);

        // Очищаем таблицы перед каждым тестом
        Context.Applications.RemoveRange(Context.Applications);
        Context.Employees.RemoveRange(Context.Employees);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}