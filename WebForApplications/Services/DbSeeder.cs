using Bogus;
using EFCore.BulkExtensions;
using WebForApplications.Models;

namespace WebForApplications.Services
{
    /// <summary>
    /// Класс для заполнения базы данных случайными значениям
    /// </summary>
    public static class DbSeeder
    {
        private static readonly string[] Divisions =
            ["IT", "HR", "Бухгалтерия", "Маркетинг", "Продажи", "Юристы", "Логистика", "Безопасность"];

        private static readonly string[] JobTitles =
            ["Специалист", "Менеджер", "Инженер", "Аналитик", "Руководитель направления", "Директор"];

        private static readonly string[] Descriptions =
        [
            "Не работает принтер в кабинете",
            "Прошу предоставить доступ к папке на сервере",
            "Нужно настроить почту на новом ноутбуке",
            "Замена картриджа",
            "Разблокировать учетную запись",
            "Оформить пропуск для нового сотрудника",
            "Проверить отчет за прошлый квартал",
            "Провести инструктаж по технике безопасности"
        ];

        /// <summary>
        /// Метод для добавления в пустую таблицу 1к новых сотрудников и 1 млн новых заявок
        /// </summary>
        /// <param name="context"></param>
        public static void SeedData(AppDbContext context)
        {
            if (context.Employees.Any())
            {
                Console.WriteLine("База данных уже содержит данные");
                return;
            }

            var employeeFaker = new Faker<Employee>("ru") // настройки инициализации для сотрудников
                .CustomInstantiator(f => new Employee(
                    name: f.Name.FullName(),
                    division: f.PickRandom(Divisions),
                    jobTitle: f.PickRandom(JobTitles)
                ));

            List<Employee> employees = employeeFaker.Generate(1000);

            context.BulkInsert(employees); // добавляем пакетом, минуя ORM

            // Вытаскиваем сгенерированные БД id для добавления в заявки
            List<int> employeeIds = context.Employees.Select(e => e.Id).ToList();


            int totalApplications = 1_000_000;
            int batchSize = 100_000; // Размер одного пакета
            var random = new Random();
            var baseDate = DateTime.UtcNow.AddDays(-1000); // стартовая дата

            for (int inserted = 0; inserted < totalApplications; inserted += batchSize)
            {
                var applicationsBatch = new List<Application>(batchSize);

                for (int i = 0; i < batchSize; i++)
                {
                    int authorId = employeeIds[random.Next(employeeIds.Count)];

                    // 30% шансов, что исполнителя нет
                    int? executorId = random.NextDouble() > 0.3
                        ? employeeIds[random.Next(employeeIds.Count)]
                        : null;

                    var createdAt = baseDate
                        .AddDays(random.Next(0, 1000))
                        .AddSeconds(random.Next(0, 86400));

                    // 30% шансов, что нет дедлайна
                    DateTime? deadline = random.NextDouble() > 0.3
                        ? createdAt.AddDays(random.Next(1, 100))
                        : null;

                    string description = Descriptions[random.Next(Descriptions.Length)];
                    int statusId = random.Next(1, 4); // 1 - Новая, 2 - В работе, 3 - Выполнена

                    var app = new Application(
                        authorId: authorId,
                        description: description,
                        createdAt: createdAt,
                        statusId: statusId,
                        deadline: deadline,
                        executorId: executorId
                    );
                    applicationsBatch.Add(app);
                }
                // Пакетная вставка
                context.BulkInsert(applicationsBatch);
            }
        }
    }
}
