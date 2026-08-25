using FluentAssertions;
using WebForApplications.Models;
using WebForApplications.Services;

namespace WebForApplications.Tests
{
    public class ReportServiceTests : TestBase
    {
        private readonly ReportService _service;
        public ReportServiceTests(PostgresFixture fixture) : base(fixture)
        {
            _service = new ReportService(Context);

            AddEmployees();
        }

        protected Employee author, executor1, executor2;
        protected void AddEmployees()
        {
            author = new Employee { Name = "Gregor", Division = "IT", JobTitle = "Developer" };
            executor1 = new Employee { Name = "Chris", Division = "IT", JobTitle = "Developer" };
            executor2 = new Employee { Name = "Gregor", Division = "HR", JobTitle = "Manager" };
            Context.Employees.AddRange(author, executor1, executor2);
            Context.SaveChanges();
        }

        [Fact]
        public void CountNumberOfOverdueApplications_InProgComplNoDeadline()
        {
            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;
            int inProgressStatusId = (int)ApplicationStatus.AllowedStatuses.InProgress;

            Context.Applications.AddRange(
                new Application(  // ОК
                    author,
                    "Make coffee",
                    statusId: inProgressStatusId,
                    deadline: DateTime.UtcNow.AddDays(-2)
                ),
                new Application( // Not OK
                    author,
                    "Make tea",
                    statusId: completedStatusId,
                    deadline: DateTime.UtcNow.AddDays(-2)
                ),
                new Application( // Not OK
                    author,
                    "Make impact",
                    statusId: inProgressStatusId,
                    deadline: DateTime.UtcNow.AddDays(2)
                ),
                new Application( // Not OK
                    author,
                    "Make America greate again",
                    statusId: inProgressStatusId,
                    deadline: null
                )
            );
            Context.SaveChanges();

            var count = _service.CountNumberOfOverdueApplications();

            count.Should().Be(1);
        }

        [Fact]
        public void CountNumberOfOverdueApplications_NewAndInProgStatuses()
        {
            int inProgressStatusId = (int)ApplicationStatus.AllowedStatuses.InProgress;
            int newStatusId = (int)ApplicationStatus.AllowedStatuses.New;

            Context.Applications.AddRange(
                new Application( // ОК
                    author,
                    "Make coffee",
                    statusId: newStatusId,
                    deadline: DateTime.UtcNow.AddDays(-2)
                ),
                new Application( // ОК
                    author,
                    "Make tea",
                    statusId: inProgressStatusId,
                    deadline: DateTime.UtcNow.AddDays(-2)
                ),
                new Application( // Not OK
                    author,
                    "Make impact",
                    statusId: inProgressStatusId,
                    deadline: DateTime.UtcNow.AddDays(2)
                ),
                new Application( // Not OK
                    author,
                    "Make America greate again",
                    statusId: newStatusId,
                    deadline: DateTime.UtcNow.AddDays(2)
                )
            );
            Context.SaveChanges();

            var count = _service.CountNumberOfOverdueApplications();

            count.Should().Be(2);
        }


        [Fact]
        public void SortCompletedApplicationsByExecutor_AllCompl()
        {
            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;

            Context.Applications.AddRange(
                new Application( // ОК
                    author,
                    "Make coffee",
                    statusId: completedStatusId,
                    executor: author
                ),
                new Application( // ОК
                    author,
                    "Make tea",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application( // ОК
                    author,
                    "Make impact",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application( // ОК
                    author,
                    "Make America greate again",
                    statusId: completedStatusId,
                    executor: executor2
                )
            );
            Context.SaveChanges();

            List<ExecutorAppCount> result = _service.SortCompletedApplicationsByExecutor();

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(3);
            result[0].Should().NotBeNull();
            result[0].Should().Be(new ExecutorAppCount(executor1, 2)); // рекорды сравниваются по значению
        }

        [Fact]
        public void SortCompletedApplicationsByExecutor_NotAllCompl()
        {
            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;
            int inProgressStatusId = (int)ApplicationStatus.AllowedStatuses.InProgress;
            int newStatusId = (int)ApplicationStatus.AllowedStatuses.New;

            Context.Applications.AddRange(
                new Application( // Not OK
                    author,
                    "Make coffee",
                    statusId: inProgressStatusId,
                    executor: author
                ),
                new Application( // OK
                    author,
                    "Make tea",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application( // OK
                    author,
                    "Make impact",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application( // Not OK
                    author,
                    "Make America greate again",
                    statusId: newStatusId,
                    executor: executor2
                )
            );
            Context.SaveChanges();

            List<ExecutorAppCount> result = _service.SortCompletedApplicationsByExecutor();

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(1);
            result[0].Should().NotBeNull();
            result[0].Should().Be(new ExecutorAppCount(executor1, 2)); // рекорды сравниваются по значению
        }

        [Fact]
        public void SortCompletedApplicationsByExecutor_Empty()
        {
            int inProgressStatusId = (int)ApplicationStatus.AllowedStatuses.InProgress;
            int newStatusId = (int)ApplicationStatus.AllowedStatuses.New;

            Context.Applications.AddRange(
                new Application( // Not OK
                    author,
                    "Make coffee",
                    statusId: inProgressStatusId,
                    executor: author
                ),
                new Application( // Not OK
                    author,
                    "Make tea",
                    statusId: inProgressStatusId,
                    executor: executor1
                ),
                new Application( // Not OK
                    author,
                    "Make impact",
                    statusId: inProgressStatusId,
                    executor: executor1
                ),
                new Application( // Not OK
                    author,
                    "Make America greate again",
                    statusId: newStatusId,
                    executor: executor2
                )
            );
            Context.SaveChanges();

            List<ExecutorAppCount> result = _service.SortCompletedApplicationsByExecutor();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void SortCompletedApplicationsByExecutor_NoExecutor()
        {
            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;

            Context.Applications.AddRange(
                new Application( // OK
                    author,
                    "Make coffee",
                    statusId: completedStatusId,
                    executor: author
                ),
                new Application( // OK
                    author,
                    "Make tea",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application( // OK
                    author,
                    "Make impact",
                    statusId: completedStatusId,
                    executor: executor1
                ),
                new Application( // Not OK
                    author,
                    "Make America greate again",
                    statusId: completedStatusId
                )
            );
            Context.SaveChanges();

            List<ExecutorAppCount> result = _service.SortCompletedApplicationsByExecutor();

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(2);
            result[0].Should().NotBeNull();
            result[1].Should().NotBeNull();
            result[1].Should().Be(new ExecutorAppCount(author, 1)); // рекорды сравниваются по значению
        }

        [Fact]
        public void SortCompletedApplicationsByExecutor_MoreThen20()
        {
            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;

            executor1.Name = $"Gregor_{1}";
            Employee[] arrayOfEmp = new Employee[50];
            for (int i = 2; i < arrayOfEmp.Length; i++)
            {
                arrayOfEmp[i] = new Employee(name: $"Gregor_{i}", division: "IT", jobTitle: "Developer");
            }
            Context.SaveChanges();

            Application[] arrayOfApp = new Application[50];
            arrayOfApp[0] = new Application( // OK
                    author,
                    $"Make coffee. {1} sugar",
                    statusId: completedStatusId,
                    executor: executor1
                );
            arrayOfApp[1] = new Application( // OK
                    author,
                    $"Make tea. {1} sugar",
                    statusId: completedStatusId,
                    executor: executor1
                );

            for (int i = 2; i < arrayOfApp.Length; i++)
            {
                arrayOfApp[i] = new Application( // OK
                    author,
                    $"Make coffee. {i} sugar",
                    statusId: completedStatusId,
                    executor: arrayOfEmp[i]
                );
            }

            Context.Applications.AddRange(arrayOfApp);
            Context.SaveChanges();

            List<ExecutorAppCount> result = _service.SortCompletedApplicationsByExecutor();

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(20);
            result[0].Should().NotBeNull();
            result[0].Should().Be(new ExecutorAppCount(executor1, 2)); // рекорды сравниваются по значению
            result[1].Should().NotBeNull();
            result[1].Executor.Division.Should().Be("IT");
            
        }


        [Fact]
        public void FindApplicationByStatus_AllStatusesOk()
        {
            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;
            int inProgressStatusId = (int)ApplicationStatus.AllowedStatuses.InProgress;
            int newStatusId = (int)ApplicationStatus.AllowedStatuses.New;

            Context.Applications.AddRange(
                new Application(author, "Make coffee", statusId: completedStatusId),
                new Application(author, "Make tea", statusId: completedStatusId),
                new Application(author, "Make impact", statusId: completedStatusId),
                new Application(author, "Make America greate again", statusId: completedStatusId),
                new Application(author, "Make coffee", statusId: completedStatusId),
                new Application(author, "Make tea", statusId: completedStatusId),
                new Application(author, "Make impact", statusId: completedStatusId), // 7
                new Application(author, "Make America greate again", statusId: inProgressStatusId),
                new Application(author, "Make coffee", statusId: inProgressStatusId),
                new Application(author, "Make tea", statusId: inProgressStatusId), // 3
                new Application(author, "Make impact", statusId: newStatusId),
                new Application(author, "Make America greate again", statusId: newStatusId) // 2
            );
            Context.SaveChanges();

            Dictionary<string, int> result = _service.FindApplicationByStatus();

            result.Should().NotBeNullOrEmpty();
            result["Новая"].Should().Be(2);
            result["В работе"].Should().Be(3);
            result["Выполнена"].Should().Be(7);
        }


        [Fact]
        public void FindApplicationByStatus_OneStatusesEmpty()
        {
            int completedStatusId = (int)ApplicationStatus.AllowedStatuses.Completed;
            int inProgressStatusId = (int)ApplicationStatus.AllowedStatuses.InProgress;

            Context.Applications.AddRange(
                new Application(author, "Make coffee", statusId: completedStatusId),
                new Application(author, "Make tea", statusId: completedStatusId),
                new Application(author, "Make impact", statusId: completedStatusId),
                new Application(author, "Make America greate again", statusId: completedStatusId),
                new Application(author, "Make coffee", statusId: completedStatusId),
                new Application(author, "Make tea", statusId: completedStatusId),
                new Application(author, "Make impact", statusId: completedStatusId), // 7
                new Application(author, "Make America greate again", statusId: inProgressStatusId),
                new Application(author, "Make coffee", statusId: inProgressStatusId),
                new Application(author, "Make tea", statusId: inProgressStatusId) // 3
            );
            Context.SaveChanges();

            Dictionary<string, int> result = _service.FindApplicationByStatus();

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(3);
            result["Новая"].Should().Be(0);
            result["В работе"].Should().Be(3);
            result["Выполнена"].Should().Be(7);
        }

        [Fact]
        public void FindApplicationByStatus_NoApplications()
        {
            Dictionary<string, int> result = _service.FindApplicationByStatus();

            result.Should().NotBeNullOrEmpty();
            result["Новая"].Should().Be(0);
            result["В работе"].Should().Be(0);
            result["Выполнена"].Should().Be(0);
        }
    }
}
