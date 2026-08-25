using Mapster;
using Microsoft.AspNetCore.Mvc;
using WebForApplications.DTOs;
using WebForApplications.Models;
using WebForApplications.Services;

namespace WebForApplications.Controllers
{
    /// <summary>
    /// Класс для эндпоинтов, отвечающих за работу с заявками и исполнителями
    /// </summary>
    [ApiController]
    [Route("api/applications")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationRepository _repository; // доступ к классу, реализующему работу с БД
        public ApplicationsController(IApplicationRepository repository)
        {
            _repository = repository;
        }

        [HttpPatch("{appId:int}/status")]
        [Tags("Заявки")]
        [EndpointSummary("Изменение статуса заявки")]
        [EndpointDescription("Изменяет значение статуса заявки в БД, если это позволяет бизнес-логика.")]
        public IActionResult ChangeApplicationStatus(int appId, [FromBody] StatusUpdate dto)
        {
            Application? app = _repository.GetApplicationById(appId);
            if (app == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "В базе данных нет заявки с таким Id");
            }

            if(!app.ChangeStatus(dto.StatusId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Невозможно изменить статус заявки " +
                    $"с {_repository.GetStatusLabel(app.StatusId)} " +
                    $"на {_repository.GetStatusLabel(dto.StatusId)}");
            }

            _repository.SaveChanges();
            return NoContent();
        }

        [HttpGet]
        [Tags("Заявки")]
        [EndpointSummary("Фильтрация заявок")]
        [EndpointDescription("Возвращает список заявок с фильтрацией по переданным параметрам.")]
        public ActionResult<List<ApplicationItemDto>> GetApplicationList([FromQuery] FilterForApplications dto)
        {
            var apps = _repository.FilterApplications
                (
                    statusId: dto.StatusId,
                    executorId: dto.ExecutorId,
                    division: dto.Division,
                    isOverdue: dto.IsOverdue
                );
            List<ApplicationItemDto> response = apps.Adapt<List<ApplicationItemDto>>();
            return Ok(response);
        }

        [HttpPost("/api/employees")]
        [Tags("Сотрудники")]
        [EndpointSummary("Добавляем нового сотрудника")]
        [EndpointDescription("Создает нового сотрудника в базе данных и возвращает его ID.")]
        public ActionResult<int> CreateEmployee([FromBody] EmployeeCreate dto)
        {
            Employee newEmp = new Employee(
                name: dto.Name,
                division: dto.Division,
                jobTitle: dto.JobTitle
            );

            _repository.AddEmployee(newEmp);
            _repository.SaveChanges();

            return Ok(newEmp.Id);
        }

        [HttpGet("/api/employees/search")]
        [Tags("Сотрудники")]
        [EndpointSummary("Поиск сотрудников")]
        [EndpointDescription("Возвращает всех сотрудников с указанным именем (частичное или полное совпадение).")]
        public ActionResult<List<EmployeeItemDto>> GetEmployeesByName([FromQuery] ExecutorNameFind dto)
        {
            var apps = _repository.FindEmployeeByName(dto.Name);
            List<EmployeeItemDto> response = apps.Adapt<List<EmployeeItemDto>>();
            return Ok(response);
        }

        [HttpPut("{appId:int}/executor")]
        [Tags("Заявки")]
        [EndpointSummary("Меняем исполнителя заявки по ID")]
        [EndpointDescription("Меняет исполнителя заявки на указанного по его id.")]
        public ActionResult<int?> ChangeApplicationsExecutor(int appId, [FromBody] ExecutorIdUpdate dto)
        {
            Application? app = _repository.GetApplicationById(appId);
            if (app == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "В базе данных нет заявки с таким Id");
            }
            int? prevExec = app.ExecutorId;
            if (!app.ChangeExecutor(dto.ExecutorId, _repository)) // безопасное изменение закрытого поля через метод интерфейса
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Сотрудник с таким Id не найден");
            }

            _repository.SaveChanges();
            return Ok(prevExec);
        }
    }
}
