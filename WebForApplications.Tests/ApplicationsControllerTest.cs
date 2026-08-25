using Bogus.DataSets;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebForApplications.Controllers;
using WebForApplications.DTOs;
using WebForApplications.Models;
using WebForApplications.Services;

namespace WebForApplications.Tests
{
    public class ApplicationsControllerTest
    {
        private readonly Mock<IApplicationRepository> _mockApplicationRepository;
        private readonly ApplicationsController _controller;

        public ApplicationsControllerTest()
        {
            _mockApplicationRepository = new Mock<IApplicationRepository>();
            _controller = new ApplicationsController(_mockApplicationRepository.Object);
        }


        [Fact]
        public void ChangeApplicationStatus_ApplicationNotFound_Returns404NotFound()
        {
            int appId = 404;
            StatusUpdate dto = new StatusUpdate { StatusId = 5};
            Application? expectedAnswer = null;
            
            _mockApplicationRepository
                .Setup(s => s.GetApplicationById(appId))
                .Returns(expectedAnswer);

            var actionResult = _controller.ChangeApplicationStatus(appId, dto);

            var result = actionResult as ObjectResult;
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);

            _mockApplicationRepository.Verify(s => s.GetApplicationById(appId), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Never);
        }

        [Fact]
        public void ChangeApplicationStatus_StatusCannotBeChanged_Returns400BadRequest()
        {
            int appId = 100;
            StatusUpdate dto = new StatusUpdate { StatusId = 5 };
            Application? expectedAnswer = new Application(
                new Employee { Name = "George", Division = "IT", Id = 24, JobTitle = "Manager"}, 
                "Make coffee", statusId: 2);

            _mockApplicationRepository
                .Setup(s => s.GetApplicationById(appId))
                .Returns(expectedAnswer);
            _mockApplicationRepository.Setup(s => s.GetStatusLabel(2)).Returns("В работе");
            _mockApplicationRepository.Setup(s => s.GetStatusLabel(5)).Returns("Неизвестно");

            var actionResult = _controller.ChangeApplicationStatus(appId, dto);

            var result = actionResult as ObjectResult;
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);

            _mockApplicationRepository.Verify(s => s.GetApplicationById(appId), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Never);
        }

        [Fact]
        public void ChangeApplicationStatus_StatusChanged_ReturnsNoContent()
        {
            int appId = 100;
            StatusUpdate dto = new StatusUpdate { StatusId = 3 };
            Application? expectedAnswer = new Application(
                new Employee { Name = "George", Division = "IT", Id = 24, JobTitle = "Manager" },
                "Make coffee", statusId: 2);

            _mockApplicationRepository
                .Setup(s => s.GetApplicationById(appId))
                .Returns(expectedAnswer);

            var actionResult = _controller.ChangeApplicationStatus(appId, dto);

            var result = actionResult as StatusCodeResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status204NoContent);

            _mockApplicationRepository.Verify(s => s.GetApplicationById(appId), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Once);
        }
    
        [Fact]
        public void GetApplicationList_ResultOk_Empty()
        {
            FilterForApplications dto = new FilterForApplications();
            _mockApplicationRepository.Setup(s => s.FilterApplications(
                dto.StatusId,
                dto.ExecutorId,
                dto.Division,
                dto.IsOverdue)
            ).Returns([]);

            var actionResult = _controller.GetApplicationList(dto);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var returnedData = result.Value as List<ApplicationItemDto>;
            returnedData.Should().NotBeNull();
            returnedData.Should().BeEmpty();

            _mockApplicationRepository.Verify(s => s.FilterApplications(
                dto.StatusId,
                dto.ExecutorId,
                dto.Division,
                dto.IsOverdue)
            , Times.Once);
        }

        [Fact]
        public void GetApplicationList_ResultOk_ReturnsList()
        {
            FilterForApplications dto = new FilterForApplications();
            _mockApplicationRepository.Setup(s => s.FilterApplications(
                dto.StatusId,
                dto.ExecutorId,
                dto.Division,
                dto.IsOverdue)
            ).Returns(
                [
                    new Application(
                        author: new Employee { Name = "George", Division = "IT", Id = 24, JobTitle = "Manager"},
                        description: "Make coffee",
                        statusId: 2),
                    new Application(
                        author: new Employee { Name = "Peter", Division = "HR", Id = 26, JobTitle = "Manager"},
                        description: "Make tea",
                        statusId: 1),
                ]);

            var actionResult = _controller.GetApplicationList(dto);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var returnedData = result.Value as List<ApplicationItemDto>;
            returnedData.Should().NotBeNullOrEmpty();
            returnedData.Count.Should().Be(2);
            returnedData[0].AuthorName.Should().Be("George");
            returnedData[1].Description.Should().Be("Make tea");

            _mockApplicationRepository.Verify(s => s.FilterApplications(
                dto.StatusId,
                dto.ExecutorId,
                dto.Division,
                dto.IsOverdue)
            , Times.Once);
        }

        [Fact]
        public void CreateEmployee_ResultOk_ReturnsId()
        {
            EmployeeCreate dto = new EmployeeCreate { 
                Division = "Legal", JobTitle = "Manager", Name = "Gregory"};

            _mockApplicationRepository
                .Setup(s => s.AddEmployee(It.IsAny<Employee>()))
                .Callback<Employee>(emp =>
                {
                    var prop = typeof(Employee).GetProperty(nameof(Employee.Id));
                    prop?.SetValue(emp, 257);
                })
                .Returns(257);

            var actionResult = _controller.CreateEmployee(dto);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var returnedData = result.Value;
            returnedData.Should().Be(257);

            _mockApplicationRepository.Verify(s => s.AddEmployee(It.IsAny<Employee>()), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetEmployeesByName_ResultOk_ReturnsList()
        {
            ExecutorNameFind dto = new ExecutorNameFind { Name = "Greg"};
            List<Employee> testEmps = [
                new Employee { Division = "Legal", JobTitle = "Manager", Name = "Gregory", Id = 54 },
                new Employee { Name = "Gregor", Division = "IT", Id = 24, JobTitle = "Manager" },
                new Employee { Name = "Peter Gregorson", Division = "HR", Id = 26, JobTitle = "Manager" }
            ];


            _mockApplicationRepository.Setup(s => s.FindEmployeeByName(dto.Name)).Returns(testEmps);

            var actionResult = _controller.GetEmployeesByName(dto);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var returnedData = result.Value as List<EmployeeItemDto>;
            returnedData.Should().NotBeNullOrEmpty();
            returnedData.Count.Should().Be(3);
            returnedData[0].Name.Should().Be("Gregory");
            returnedData[1].Division.Should().Be("IT");
            returnedData[1].JobTitle.Should().Be("Manager");
            returnedData[2].Id.Should().Be(26);

            _mockApplicationRepository.Verify(s => s.FindEmployeeByName(dto.Name), Times.Once);
        }

        [Fact]
        public void GetEmployeesByName_ResultOk_ReturnsEmpty()
        {
            ExecutorNameFind dto = new ExecutorNameFind { Name = "Greg" };

            _mockApplicationRepository.Setup(s => s.FindEmployeeByName(dto.Name)).Returns([]);

            var actionResult = _controller.GetEmployeesByName(dto);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var returnedData = result.Value as List<EmployeeItemDto>;
            returnedData.Should().BeEmpty();
            returnedData.Should().NotBeNull();

            _mockApplicationRepository.Verify(s => s.FindEmployeeByName(dto.Name), Times.Once);
        }

        [Fact]
        public void ChangeApplicationsExecutor_ApplicationNotFound_Returns404NotFound()
        {
            int appId = 404;
            ExecutorIdUpdate testExecId = new ExecutorIdUpdate { ExecutorId = 202};
            Application? expectedAnswer = null;

            _mockApplicationRepository.Setup(s => s.GetApplicationById(appId)).Returns(expectedAnswer);

            var actionResult = _controller.ChangeApplicationsExecutor(appId, testExecId);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);

            _mockApplicationRepository.Verify(s => s.GetApplicationById(appId), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Never);
        }

        [Fact]
        public void ChangeApplicationsExecutor_ExecutorDoesNotExist_Returns400BadRequest()
        {
            int appId = 404;
            ExecutorIdUpdate testExecId = new ExecutorIdUpdate { ExecutorId = 999999999 };
            Application? expectedAnswer = new Application(
                new Employee { Name = "George", Division = "IT", Id = 24, JobTitle = "Manager" },
                "Make coffee", statusId: 2);

            _mockApplicationRepository.Setup(s => s.GetApplicationById(appId)).Returns(expectedAnswer);
            _mockApplicationRepository.Setup(s => s.IsEmployeeExists(testExecId.ExecutorId)).Returns(false);

            var actionResult = _controller.ChangeApplicationsExecutor(appId, testExecId);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);

            _mockApplicationRepository.Verify(s => s.GetApplicationById(appId), Times.Once);
            _mockApplicationRepository.Verify(s => s.IsEmployeeExists(testExecId.ExecutorId), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Never);
        }

        [Fact]
        public void ChangeApplicationsExecutor_ExecutorChanged_ReturnsNull()
        {
            int appId = 404;
            ExecutorIdUpdate testExecId = new ExecutorIdUpdate { ExecutorId = 7432 };
            Application? expectedAnswer = new Application(
                new Employee { Name = "George", Division = "IT", Id = 24, JobTitle = "Manager" },
                "Make coffee", statusId: 2);

            _mockApplicationRepository.Setup(s => s.GetApplicationById(appId)).Returns(expectedAnswer);
            _mockApplicationRepository.Setup(s => s.IsEmployeeExists(testExecId.ExecutorId)).Returns(true);

            var actionResult = _controller.ChangeApplicationsExecutor(appId, testExecId);

            var result = actionResult.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedData = (int?)result.Value;
            returnedData.Should().BeNull();

            _mockApplicationRepository.Verify(s => s.GetApplicationById(appId), Times.Once);
            _mockApplicationRepository.Verify(s => s.IsEmployeeExists(testExecId.ExecutorId), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Once);
        }

        [Fact]
        public void ChangeApplicationsExecutor_ExecutorChanged_ReturnsId()
        {
            int appId = 404;
            ExecutorIdUpdate testExecId = new ExecutorIdUpdate { ExecutorId = 7432 };
            Application? expectedAnswer = new Application(
                authorId: 24,
                "Make coffee",
                statusId: 2,
                executorId: 100);

            _mockApplicationRepository.Setup(s => s.GetApplicationById(appId)).Returns(expectedAnswer);
            _mockApplicationRepository.Setup(s => s.IsEmployeeExists(testExecId.ExecutorId)).Returns(true);

            var actionResult = _controller.ChangeApplicationsExecutor(appId, testExecId);

            var result = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);

            result.Value.Should().NotBeNull();
            result.Value.Should().Be(100);

            _mockApplicationRepository.Verify(s => s.GetApplicationById(appId), Times.Once);
            _mockApplicationRepository.Verify(s => s.IsEmployeeExists(testExecId.ExecutorId), Times.Once);
            _mockApplicationRepository.Verify(s => s.SaveChanges(), Times.Once);
        }
    }
}
