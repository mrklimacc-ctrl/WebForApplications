using Docker.DotNet.Models;
using FluentAssertions;
using WebForApplications.Models;
using WebForApplications.Services;

namespace WebForApplications.Tests
{
    public class ApplicationRepositoryTests : TestBase
    {
        private readonly ApplicationRepository _repository;

        public ApplicationRepositoryTests(PostgresFixture fixture) : base(fixture)
        {
            _repository = new ApplicationRepository(Context);

            InitializeTestDB();
        }

        protected Employee author, executor1, executor2;
        Application application1;
        protected void InitializeTestDB()
        {
            author = new Employee { Name = "Gregor", Division = "IT", JobTitle = "Developer" };
            executor1 = new Employee { Name = "Chris", Division = "IT", JobTitle = "Developer" };
            executor2 = new Employee { Name = "Gregor", Division = "HR", JobTitle = "Manager" };
            Context.Employees.AddRange(author, executor1, executor2);
            Context.SaveChanges();

            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;
            int inProgressStatusId = (int)ApplicationStatus.AllowedStatuses.InProgress;
            int newStatusId = (int)ApplicationStatus.AllowedStatuses.New;

            application1 = new Application(author, "Make tests", statusId: completedStatusId, executor: executor1);

            Context.Applications.AddRange(
                new Application(
                    author,
                    "Make coffee",
                    statusId: inProgressStatusId,
                    executor: author,
                    deadline: DateTime.UtcNow.AddDays(-6)
                ),
                new Application(
                    author,
                    "Make tea",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application(
                    author,
                    "Make impact",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application(
                    author,
                    "Make America greate again",
                    statusId: newStatusId,
                    executor: executor2,
                    deadline: DateTime.UtcNow.AddDays(6)
                ),
                application1
            );
            Context.SaveChanges();
        }

        [Fact]
        public void GetApplicationById_ReturnsApplication()
        {
            int appId = application1.Id;

            Application? result = _repository.GetApplicationById(appId);

            result.Should().NotBeNull();
            result.Id.Should().Be(appId);
            result.Description.Should().Be(application1.Description);
            result.Author.Id.Should().Be(author.Id);
            result.Executor!.Id.Should().Be(executor1.Id);
            result.Status.Should().Be(application1.Status);
        }

        [Fact]
        public void GetApplicationById_NoApplication()
        {
            int appId = -1;

            Application? result = _repository.GetApplicationById(appId);

            result.Should().BeNull();
        }

        [Fact]
        public void FilterApplications_NoFilters()
        {
            int? statusId = null;
            int? executorId = null;
            string? division = null;
            bool isOverdue = false;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }

        [Fact]
        public void FilterApplications_AllFilters()
        {
            Context.Applications.Add(new Application(
                    executor2,
                    "Make smthg",
                    statusId: (int)ApplicationStatus.AllowedStatuses.InProgress,
                    executor: executor1,
                    deadline: DateTime.UtcNow.AddDays(-10)
                ));
            Context.SaveChanges();

            int? statusId = (int)ApplicationStatus.AllowedStatuses.InProgress;
            int? executorId = executor1.Id;
            string? division = "IT";
            bool isOverdue = true;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNullOrEmpty();
            result[0].Should().NotBeNull();
            result[0].Author.Id.Should().Be(executor2.Id);
            result[0].Description.Should().Be("Make smthg");
        }

        [Fact]
        public void FilterApplications_AllFiltersEmpty()
        {
            int? statusId = application1.StatusId;
            int? executorId = author.Id;
            string? division = "HR";
            bool isOverdue = true;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterApplications_StatusId()
        {
            int? statusId = application1.StatusId;
            int? executorId = null;
            string? division = null;
            bool isOverdue = false;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(3);
            result[1].Should().NotBeNull();
            result[1].Executor!.Id.Should().Be(executor1.Id); // у всех трех и только у них, порядок не важен
        }

        [Fact]
        public void FilterApplications_StatusIdEmpty()
        {
            int? statusId = 100;
            int? executorId = null;
            string? division = null;
            bool isOverdue = false;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterApplications_ExecutorId()
        {
            int? statusId = null;
            int? executorId = author.Id;
            string? division = null;
            bool isOverdue = false;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(1);
            result[0].Should().NotBeNull();
            result[0].Executor!.Id.Should().Be(author.Id);
            result[0].Description.Should().Be("Make coffee");
        }

        [Fact]
        public void FilterApplications_ExecutorIdEmpty()
        {
            int? statusId = null;
            int? executorId = 57464;
            string? division = null;
            bool isOverdue = false;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterApplications_Division()
        {
            int? statusId = null;
            int? executorId = null;
            string? division = "HR";
            bool isOverdue = false;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(1);
            result[0].Should().NotBeNull();
            result[0].Executor!.Id.Should().Be(executor2.Id);
            result[0].Description.Should().Be("Make America greate again");
        }

        [Fact]
        public void FilterApplications_DivisionEmpty()
        {
            int? statusId = null;
            int? executorId = null;
            string? division = "Legal";
            bool isOverdue = false;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void FilterApplications_IsOverdue()
        {
            int? statusId = null;
            int? executorId = null;
            string? division = null;
            bool isOverdue = true;

            List<Application> result = _repository.FilterApplications(statusId, executorId, division, isOverdue);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(1);
            result[0].Should().NotBeNull();
            result[0].Executor!.Id.Should().Be(author.Id);
            result[0].Description.Should().Be("Make coffee");
        }

        [Fact]
        public void FindEmployeeByName_EmptyString()
        {
            Context.AddRange([
                new Employee { Name = "Peter Gregorson", Division = "Legal", JobTitle = "Developer" },
                new Employee { Name = "Andy Peterson", Division = "Legal", JobTitle = "Developer" },
                ]);
            Context.SaveChanges();

            string name = "";

            List<Employee> result = _repository.FindEmployeeByName(name);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(5); // все сотрудники

        }

        [Fact]
        public void FindEmployeeByName_FewEmployees()
        {
            Context.AddRange([
                new Employee { Name = "Peter Gregorson", Division = "Legal", JobTitle = "Developer" },
                new Employee { Name = "Andy Peterson", Division = "Legal", JobTitle = "Developer" },
                ]);
            Context.SaveChanges();

            string name = "Peter";

            List<Employee> result = _repository.FindEmployeeByName(name);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(2);
            result[0].Should().NotBeNull();
            result[0].Division.Should().Be("Legal");
        }

        [Fact]
        public void FindEmployeeByName_NoEmployees()
        {
            string name = "Frank";

            List<Employee> result = _repository.FindEmployeeByName(name);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void AddEmployee_Ok()
        {
            Employee newEmp = new() { Name = "Mark", Division = "Security", JobTitle = "Director" };
            
            int result = _repository.AddEmployee(newEmp);

            newEmp.Id.Should().NotBe(0);
            result.Should().Be(newEmp.Id);
        }

        [Fact]
        public void GetEmployeeById_Empty()
        {
            Employee? result = _repository.GetEmployeeById(-1);

            result.Should().BeNull();
        }

        [Fact]
        public void GetEmployeeById_Ok()
        {
            Employee? result = _repository.GetEmployeeById(executor1.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(executor1.Id);
        }

        [Fact]
        public void IsEmployeeExists_Yes()
        {
            bool result = _repository.IsEmployeeExists(executor1.Id);

            result.Should().Be(true);
        }

        [Fact]
        public void IsEmployeeExists_No()
        {
            bool result = _repository.IsEmployeeExists(-1);

            result.Should().Be(false);
        }

        [Fact]
        public void GetStatusLabel_New()
        {
            string result = _repository.GetStatusLabel(1);

            result.Should().Be("Новая");
        }

        [Fact]
        public void GetStatusLabel_InProgress()
        {
            string result = _repository.GetStatusLabel(2);

            result.Should().Be("В работе");
        }

        [Fact]
        public void GetStatusLabel_Complited()
        {
            string result = _repository.GetStatusLabel(3);

            result.Should().Be("Выполнена");
        }

        [Fact]
        public void GetStatusLabel_NoSuchStatus()
        {
            string result = _repository.GetStatusLabel(5);

            result.Should().Be("Неизвестно");
        }
    }


}