using WebForApplications.Models;
using static WebForApplications.Services.ReportService;

namespace WebForApplications.Services
{
    // record для обертки "кортежа" (Исполнитель, число_заявок) в запросе
    public record ExecutorAppCount(Employee Executor, int Count);

    /// <summary>
    /// Интерфейс для удобства при создания юнит-тестов контроллера
    /// </summary>
    public interface IReportService
    {
        public Dictionary<string, int> FindApplicationByStatus();
        public int CountNumberOfOverdueApplications();
        public List<ExecutorAppCount> SortCompletedApplicationsByExecutor();
    }

    /// <summary>
    /// Класс для генерации отчетов
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context; // доступ к БД
        public ReportService(AppDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Для всех статусов из таблицы application_status считает число заявок в таблице application
        /// </summary>
        /// <returns>Возвращает словарь формата [имя_статуса] -> [число_заявок]</returns>
        public Dictionary<string, int> FindApplicationByStatus()
        {
            return _context.ApplicationStatuses
            .Select(s => new
            {
                StatusName = s.Name,
                Count = _context.Applications.Count(a => a.StatusId == s.Id)
            })
            .ToDictionary(x => x.StatusName, x => x.Count);
        }
        /// <summary>
        /// Считает количество просроченных заявок (если установлен дедлайн)
        /// </summary>
        /// <returns>Возвращает количество невыполненных заявок с просроченным дедлайном</returns>
        public int CountNumberOfOverdueApplications()
        {
            return _context.Applications.Count(a => a.Deadline != null
                && a.StatusId != (int)ApplicationStatus.AllowedStatuses.Completed
                && DateTime.UtcNow > a.Deadline);
        }

        
        /// <summary>
        /// Составляет топ-20 самых активных исполнителей
        /// </summary>
        /// <returns>Возвращает список "кортежей" (Исполнитель, число_заявок) 
        /// с сортировкой по кол-ву завершенных заявок (если их больше нуля)</returns>
        public List<ExecutorAppCount> SortCompletedApplicationsByExecutor()
        {
            return _context.Applications
                .Where(a => a.StatusId == (int)ApplicationStatus.AllowedStatuses.Completed && a.ExecutorId != null)
                .GroupBy(a => a.ExecutorId)
                .Select(g => new
                {
                    ExecutorId = g.Key!.Value,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(20)
                .Join(
                    _context.Employees,
                    groupResult => groupResult.ExecutorId,
                    employee => employee.Id,
                    (groupResult, employee) => new
                    {
                        Executor = employee,
                        groupResult.Count
                    }
                )
                .AsEnumerable()
                .Select(x => new ExecutorAppCount(x.Executor, x.Count))
                .ToList();
        }
    }
}
