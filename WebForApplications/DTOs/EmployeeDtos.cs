using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebForApplications.DTOs
{
    /* Схемы Data Transfer Objects для проверки входных\выходных данных о сотрудниках */

    /// <summary>
    /// Проверка данных при создании нового сотрудника для эндпоинта /api/employees
    /// </summary>
    public class EmployeeCreate
    {
        /// <summary>
        /// ФИО нового сотрудника
        /// </summary>
        /// <example>Иванов Иван Иванович</example>
        [Required(ErrorMessage = "Имя сотрудника обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина имени должна быть от 2 до 100 символов")]
        [Description("ФИО сотрудника")]
        public required string Name { get; init; }

        /// <summary>
        /// Отдел
        /// </summary>
        /// <example>IT</example>
        [Required(ErrorMessage = "Указание отдела обязательно")]
        [MaxLength(100, ErrorMessage = "Название отдела не может превышать 100 символов")]
        [Description("Отдел")]
        public required string Division { get; init; }

        /// <summary>
        /// Должность
        /// </summary>
        /// <example>Разработчик</example>
        [Required(ErrorMessage = "Указание должности обязательно")]
        [MaxLength(100, ErrorMessage = "Название должности не может превышать 100 символов")]
        [Description("Должность")]
        public required string JobTitle { get; init; }
    }

    /// <summary>
    /// Проверка имени исполнителя для эндпоинта /api/employees/search
    /// </summary>
    public class ExecutorNameFind
    {
        /// <summary>
        /// ФИО нового исполнителя
        /// </summary>
        /// <example>Иванов Иван Иванович</example>
        [Required(ErrorMessage = "Имя исполнителя обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина имени должна быть от 2 до 100 символов")]
        [Description("ФИО исполнителя")]
        public required string Name { get; init; }
    }

    /// <summary>
    /// Схема ответа с данными о количестве заявок у исполнителя
    /// </summary>
    public class TopExecutorDto
    {
        public required EmployeeItemDto Employee { get; init; }
        public required int Count { get; init; }
    }
    /// <summary>
    /// Схема ответа с данными о сотруднике
    /// </summary>
    public class EmployeeItemDto
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public required string Division { get; init; }
        public required string JobTitle { get; init; }
    }
}
