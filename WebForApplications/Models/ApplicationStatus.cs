namespace WebForApplications.Models
{
    /// <summary>
    /// Класс для таблицы application_status
    /// </summary>
    public class ApplicationStatus
    {
        public enum AllowedStatuses
        {
            New = 1,
            InProgress = 2,
            Completed = 3,
        }

        public ApplicationStatus() { }

        public int Id { get; init; } = (int)AllowedStatuses.New;
        public string Name { get; init; } = "Новая";
    }

    public static class ApplicationStatusExtensions
    {
        /// <summary>
        /// Метод для получения наименования статуса заявки (в виде теста)
        /// </summary>
        public static string GetLabel(this ApplicationStatus.AllowedStatuses status) => status switch
        {
            ApplicationStatus.AllowedStatuses.New => "Новая",
            ApplicationStatus.AllowedStatuses.InProgress => "В работе",
            ApplicationStatus.AllowedStatuses.Completed => "Выполнена",
            _ => "Неизвестно"
        };
    }
}


