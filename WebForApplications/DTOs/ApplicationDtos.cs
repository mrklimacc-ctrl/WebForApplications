using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebForApplications.DTOs
{
    /* Схемы Data Transfer Objects для проверки входных\выходных данных о заявках*/

    /// <summary>
    /// Проверка фильтров StatusId, ExecutorId, Division и IsOverdue для эндпоинта api/applications
    /// </summary>
    public class FilterForApplications
    /* Проверка данных при передаче аргументов для фильтрации */
    {
        /// <summary>
        /// Фильтр для просроченных заявок
        /// </summary>
        /// <example>true</example>
        [Description("Показать просроченные (true/false)")]
        public bool IsOverdue { get; init; }

        /// <summary>
        /// Фильтр по ID статуса
        /// </summary>
        /// <example>2</example>
        [Range(1, 3, ErrorMessage = "1 - 'Новая', 2 - 'В работе', 3 - 'Завершена'")]
        [Description("Фильтр по ID статуса: 1 - 'Новая', 2 - 'В работе', 3 - 'Завершена'")]
        public int? StatusId { get; init; }

        /// <summary>
        /// Фильтр по ID исполнителя
        /// </summary>
        /// <example>245</example>
        [Range(1, int.MaxValue, ErrorMessage = "ID должен быть больше 0")]
        [Description("Фильтр по ID исполнителя")]
        public int? ExecutorId { get; init; }

        /// <summary>
        /// Фильтр по отделу
        /// </summary>
        /// <example>IT</example>
        [MaxLength(100, ErrorMessage = "Название отдела не может превышать 100 символов")]
        [Description("Фильтр по отделу")]
        public string? Division { get; init; }

    }
    /// <summary>
    /// Проверка нового статуса для эндпоинта api/applications/{appId:int}/status
    /// </summary>
    public class StatusUpdate
    {
        /// <summary>
        /// ID нового статуса
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "Id статуса обязателен")]
        [Range(1, 3, ErrorMessage = "1 - 'Новая', 2 - 'В работе', 3 - 'Завершена'")]
        [Description("ID нового статуса: 1 - 'Новая', 2 - 'В работе', 3 - 'Завершена'")]
        public int StatusId { get; init; }

    }
    /// <summary>
    /// Проверка нового Id исполнителя для эндпоинта api/applications/{appId:int}/executor
    /// </summary>
    public class ExecutorIdUpdate
    {   
        /// <summary>
        /// ID нового исполнителя
        /// </summary>
        /// <example>245</example>
        [Required(ErrorMessage = "Id исполнителя обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID должен быть больше 0")]
        [Description("ID нового исполнителя")]
        public int ExecutorId { get; init; }

    }

    /// <summary>
    /// Схема ответа с данными о заявке
    /// </summary>
    public class ApplicationItemDto
    {
        public int Id { get; init; }
        public DateTime CreatedAt { get; init; }
        public int AuthorId { get; init; }
        public string AuthorName { get; init; } = string.Empty;
        public int? ExecutorId { get; init; }
        public string? ExecutorName { get; init; }
        public string? ExecutorDivision { get; init; }
        public DateTime? Deadline { get; init; }
        public string Description { get; init; } = string.Empty;
        public string StatusName { get; init; } = string.Empty;
    }
    /// <summary>
    /// Схема ответа с количестве заявок, соответствующим статусу
    /// </summary>
    public class StatusDict
    {
        public required int NumberOfApplications { get; init; }
        public required string StatusName { get; init; }
    }

}
