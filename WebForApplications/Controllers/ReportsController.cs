using Mapster;
using Microsoft.AspNetCore.Mvc;
using WebForApplications.DTOs;
using WebForApplications.Services;
using static WebForApplications.Services.ReportService;

namespace WebForApplications.Controllers
{
    /// <summary>
    /// Класс для эндпоинтов, отвечающих за предоставление отчетов
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reports; // доступ к классу отчетов
        public ReportsController(IReportService reports)
        {
            _reports = reports;
        }

        [HttpGet("out-of-deadline")]
        [Tags("Отчеты")]
        [EndpointSummary("Количество просроченных заявок")]
        [EndpointDescription("Возвращает количество невыполненных заявок с просроченным дедлайном.")]
        public ActionResult<int> GetNumberOfOverdueApplications()
        {
            return Ok(_reports.CountNumberOfOverdueApplications());
        }

        [HttpGet("by-status")]
        [Tags("Отчеты")]
        [EndpointSummary("Количество заявок по статусам")]
        [EndpointDescription("Возвращает список имен статусов и соответствующее им число заявок.")]
        public ActionResult<List<StatusDict>> GetApplicationByStatus()
        {
            Dictionary<string, int> data = _reports.FindApplicationByStatus();
            List<StatusDict> response = data.Adapt<List<StatusDict>>();

            return Ok(response);
        }

        [HttpGet("top-executors")]
        [Tags("Отчеты")]
        [EndpointSummary("Топ-20 исполнителей по выполненным заявкам")]
        [EndpointDescription("Возвращает топ-20 исполнителей из таблицы application по кол-ву завершенных ими заявок (если их больше нуля).")]
        public ActionResult<List<TopExecutorDto>> GetExecutorsByCompleteApplications()
        {
            List<ExecutorAppCount> data = _reports.SortCompletedApplicationsByExecutor();
            List<TopExecutorDto> response = data.Adapt<List<TopExecutorDto>>();

            return Ok(response);
        }
 
    }
}
