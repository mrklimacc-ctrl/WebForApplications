using Microsoft.EntityFrameworkCore;
using WebForApplications.Models;

namespace WebForApplications.Services
{
    /// <summary>
    /// Интерфейс для удобства при создания юнит-тестов контроллера
    /// </summary>
    public interface IApplicationRepository
    {
        public Application? GetApplicationById(int applicationId);
        public List<Application> FilterApplications(
            int? statusId = null,
            int? executorId = null,
            string? division = null,
            bool isOverdue = false
        );
        public List<Employee> FindEmployeeByName(string name);
        public int AddEmployee(Employee emp);
        public string GetStatusLabel(int statusId);
        public void SaveChanges();
        /// <summary>
        /// Проверяет существование сотрудника с указанным id
        /// </summary>
        public bool IsEmployeeExists(int execId);
    }

    /// <summary>
    /// Класс, реализующий методы для базовой работы с заявками в БД в рамках бизнес-логики
    /// </summary>
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly AppDbContext _context; // доступ к БД
        public ApplicationRepository(AppDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Метод для сохранения изменений в БД
        /// </summary>
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
        /// <summary>
        /// Ищет заявку по id
        /// </summary>
        /// <returns>Возвращает заявку с подгруженными связями</returns>
        public Application? GetApplicationById(int applicationId)
        {
            return _context.Applications
                .Include(a => a.Author)
                .Include(a => a.Executor)
                .Include(a => a.Status)
                .FirstOrDefault(a => a.Id == applicationId);
        }
        /// <summary>
        /// Метод для фильтрации заявок по переданному(-ым) аргументам
        /// </summary>
        /// <returns>Возвращает список заявок (до 100) с примененными фильтрами с сортировкой по дате создания</returns>
        public List<Application> FilterApplications(
            int? statusId = null, 
            int? executorId = null, 
            string? division = null, 
            bool isOverdue = false
        )
        {
            IQueryable<Application> query = _context.Applications // составной запрос
            .Include(a => a.Author)
            .Include(a => a.Executor)
            .Include(a => a.Status);

            if (statusId.HasValue)
            {
                query = query.Where(a => a.StatusId == statusId.Value);
            }

            if (executorId.HasValue)
            {
                query = query.Where(a => a.ExecutorId == executorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(division))
            {
                query = query.Where(a => a.Executor != null && a.Executor.Division == division);
            }

            if (isOverdue)
            {
                query = query.Where(a => a.Deadline != null
                    && a.StatusId != (int)ApplicationStatus.AllowedStatuses.Completed
                    && a.Deadline < DateTime.UtcNow);
            }

            return query
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .ToList();
        }
        /// <summary>
        /// Ищет сотрудника по имени
        /// </summary>
        /// <returns>Возвращает список сотрудников с частичным или полным совпадение имени:
        /// При вводе "Иван" выведет в том числе "Иванов", "Иванович" и тп</returns>
        public List<Employee> FindEmployeeByName(string name)
        {
            return _context.Employees
                .Where(a => a.Name.Contains(name))
                .OrderBy(a => a.Division)
                .ToList();
        }
        /// <summary>
        /// Добавление в БД нового сотрудника
        /// </summary>
        /// <returns>Возвращает id нового сотрудника</returns>
        public int AddEmployee(Employee emp)
        {
            _context.Add(emp);
            _context.SaveChanges();
            return emp.Id;
        }
        /// <summary>
        /// Находит сотрудника по id
        /// </summary>
        /// <returns>Возвращает сотрудника или null</returns>
        public Employee? GetEmployeeById(int id)
        {
            return _context.Employees.FirstOrDefault(a => a.Id == id);  
        }

        /// <inheritdoc />
        public bool IsEmployeeExists(int execId)
        {
            Employee? emp = _context.Employees.FirstOrDefault(a => a.Id == execId);
            if (emp != null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Метод для получения наименования статуса заявки (в виде теста)
        /// </summary>
        public string GetStatusLabel(int statusId)
        {
            return ((ApplicationStatus.AllowedStatuses)statusId).GetLabel();
        }
    }
}
