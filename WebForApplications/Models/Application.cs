using WebForApplications.Services;

namespace WebForApplications.Models
{
    /// <summary>
    /// Класс для таблицы аpplication
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Конструктор для создания заявок через объекты сотрудников
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public Application() { }
        public Application(
            Employee author,
            string description,
            DateTime? createdAt = null,
            int statusId = (int)ApplicationStatus.AllowedStatuses.New,
            DateTime? deadline = null,
            Employee? executor = null
        )
        {
            Author = author ?? throw new ArgumentException(nameof(author));
            Description = string.IsNullOrWhiteSpace(description)
                ? throw new ArgumentException("Описание не может быть пустым", nameof(description))
                : description;
            CreatedAt = createdAt ?? DateTime.UtcNow;
            StatusId = statusId;
            Deadline = deadline;
            Executor = executor;
        }

        /// <summary>
        /// Конструктор для массовой вставки через id сотрудников
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public Application(
            int authorId,
            string description,
            DateTime? createdAt = null,
            int statusId = (int)ApplicationStatus.AllowedStatuses.New,
            DateTime? deadline = null,
            int? executorId = null
        )
            {
                AuthorId = authorId;
                Description = string.IsNullOrWhiteSpace(description)
                    ? throw new ArgumentException("Описание не может быть пустым", nameof(description))
                    : description;
                CreatedAt = createdAt ?? DateTime.UtcNow;
                StatusId = statusId;
                Deadline = deadline;
                ExecutorId = executorId;
            }

        /// <summary>
        /// Публичный метод для смены статуса с проверкой бизнес-логики
        /// </summary>
        /// <param name="newStatus"></param>
        /// <returns>Возвращает true, если удалось поменять статус и false в противном случае</returns>
        public bool ChangeStatus(int newStatus)
        {
            if (CanStatusBeChanged(newStatus))
            {
                StatusId = newStatus;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Публичный метод для смены исполнителя заявки с валидацией
        /// </summary>
        /// <returns>Возвращает true, если удалось поменять исполнителя и false в противном случае</returns>
        public bool ChangeExecutor(int newExecId, IApplicationRepository empChecker) 
            // экземпляр empChecker передается для валидации, требующей доступа к БД
        {
            if(empChecker.IsEmployeeExists(newExecId))
            {
                ExecutorId = newExecId;
                return true;
            }
            return false;
        }

        /// <summary>
        ///Карта разрешенных переходов: Текущий -> [Допустимые] 
        /// </summary>
        private static readonly Dictionary<int, HashSet<int>> AllowedTransitions = new()
        {
            [(int)ApplicationStatus.AllowedStatuses.New] = new() { (int)ApplicationStatus.AllowedStatuses.InProgress },
            [(int)ApplicationStatus.AllowedStatuses.InProgress] = new() { (int)ApplicationStatus.AllowedStatuses.Completed },
            [(int)ApplicationStatus.AllowedStatuses.Completed] = new()
        };

        protected bool CanStatusBeChanged(int newStatus)
        {
            // Проверяем, есть ли статус в списке разрешенных для текущего статуса заявки
            return AllowedTransitions.TryGetValue(StatusId, out var validNextStatuses)
                && validNextStatuses.Contains(newStatus);
        }


        public int Id { get; init; }
        public DateTime CreatedAt { get; private set; }
        public int AuthorId { get; private set; }
        public Employee Author { get; private set; } = null!;
        public int? ExecutorId { get; private set; }
        public Employee? Executor { get; set; }
        public DateTime? Deadline { get; set; }
        public string Description { get; private set; } = string.Empty;
        public int StatusId { get; private set; } = (int)ApplicationStatus.AllowedStatuses.New;
        public ApplicationStatus Status { get; private set; } = null!;
    }
}
