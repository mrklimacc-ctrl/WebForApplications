using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebForApplications.Controllers;
using WebForApplications.DTOs;
using WebForApplications.Models;
using WebForApplications.Services;

namespace WebForApplications.Tests
{
    public class ReportsControllerTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _mockReportService = new Mock<IReportService>();
            _controller = new ReportsController(_mockReportService.Object);

            TypeAdapterConfig<KeyValuePair<string, int>, StatusDict>.NewConfig()
                .Map(dest => dest.StatusName, src => src.Key)
                .Map(dest => dest.NumberOfApplications, src => src.Value);

            TypeAdapterConfig<ExecutorAppCount, TopExecutorDto>.NewConfig()
                .Map(dest => dest.Employee, src => src.Executor);
        }

        [Fact]
        public void GetApplicationByStatus_ReturnsOk_WithStatusCountsDictionary()
        {
            var expectedDictionary = new Dictionary<string, int>
            {
                { "Новая", 5 },
                { "В работе", 3 },
                { "Завершена", 12 }
            };

            _mockReportService
                .Setup(s => s.FindApplicationByStatus())
                .Returns(expectedDictionary);

            var actionResult = _controller.GetApplicationByStatus();

            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var returnedData = okResult.Value as List<StatusDict>;
            returnedData.Should().NotBeNull();
            returnedData.Should().HaveCount(3);
            returnedData[0].StatusName.Should().Be("Новая");
            returnedData[2].NumberOfApplications.Should().Be(12);

            _mockReportService.Verify(s => s.FindApplicationByStatus(), Times.Once);
        }

        [Fact]
        public void GetApplicationByStatus_ReturnsOk_Empty()
        {
            Dictionary<string, int>? expectedDictionary = new Dictionary<string, int>();

            _mockReportService
                .Setup(s => s.FindApplicationByStatus())
                .Returns(expectedDictionary);

            var actionResult = _controller.GetApplicationByStatus();

            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var returnedData = okResult.Value as List<StatusDict>;
            returnedData.Should().BeNullOrEmpty();

            _mockReportService.Verify(s => s.FindApplicationByStatus(), Times.Once);
        }

        [Fact]
        public void GetNumberOfOverdueApplications_ReturnsOk_WithCorrectNumber()
        {
            int expectedNumber = 16;

            _mockReportService
                .Setup(s => s.CountNumberOfOverdueApplications())
                .Returns(expectedNumber);

            var actionResult = _controller.GetNumberOfOverdueApplications();

            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var returnedData = (int)okResult.Value!;

            _mockReportService.Verify(s => s.CountNumberOfOverdueApplications(), Times.Once);
        }

        [Fact]
        public void GetExecutorsByCompleteApplications_ReturnsOk_WithExecutorsList()
        {
            var expectedList = new List<ExecutorAppCount>
            {
                new ExecutorAppCount ( 
                    new Employee {Id = 254, Name = "Ivan", Division = "IT", JobTitle = "Programmer" }, 25),
                new ExecutorAppCount (
                    new Employee {Id = 4354, Name = "Petr", Division = "HR", JobTitle = "Assistant" }, 23),
                new ExecutorAppCount (
                    new Employee {Id = 3, Name = "Freddy", Division = "Legal", JobTitle = "Director" }, 22)
            };

            _mockReportService
                .Setup(s => s.SortCompletedApplicationsByExecutor())
                .Returns(expectedList);

            var actionResult = _controller.GetExecutorsByCompleteApplications();

            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var returnedData = okResult.Value as List<TopExecutorDto>;
            returnedData.Should().NotBeNull();
            returnedData.Should().HaveCount(3);
            returnedData![0].Count.Should().Be(25);
            returnedData![1].Count.Should().Be(23);
            returnedData![2].Count.Should().Be(22);
            returnedData![0].Employee.Should().NotBeNull();
            returnedData![0].Employee.Name.Should().Be("Ivan");
            returnedData![2].Employee.JobTitle.Should().Be("Director");
            returnedData![2].Count.Should().Be(22);

            _mockReportService.Verify(s => s.SortCompletedApplicationsByExecutor(), Times.Once);
        }

        [Fact]
        public void GetExecutorsByCompleteApplications_ReturnsOk_Empty()
        {
            List<ExecutorAppCount>? expectedList = [];

            _mockReportService
                .Setup(s => s.SortCompletedApplicationsByExecutor())
                .Returns(expectedList);

            var actionResult = _controller.GetExecutorsByCompleteApplications();

            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var returnedData = okResult.Value as List<TopExecutorDto>;
            returnedData.Should().BeNullOrEmpty();

            _mockReportService.Verify(s => s.SortCompletedApplicationsByExecutor(), Times.Once);
        }
    }
}
