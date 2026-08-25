namespace WebForApplications.Models
{
    /// <summary>
    /// Класс для таблицы еmployee
    /// </summary>
    public class Employee
    {
        public Employee() { }
        /// <summary>
        /// Конструктор для создания объекта (все поля обязательные)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public Employee(string name, string division, string jobTitle)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Невозможно добавить сотрудника без имени!", nameof(name));
            if (string.IsNullOrEmpty(division))
                throw new ArgumentException("Невозможно добавить сотрудника без отдела!", nameof(division));
            if (string.IsNullOrEmpty(jobTitle))
                throw new ArgumentException("Невозможно добавить сотрудника без должности!", nameof(jobTitle));
            Name = name;
            Division = division;
            JobTitle = jobTitle;
        }
        public int Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }
}
