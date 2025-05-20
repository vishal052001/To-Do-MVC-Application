using Microsoft.AspNetCore.Mvc;
using ToDoListAppMVC.Filters;
using ToDoListAppMVC.Models;
using ToDoListAppMVC.Services;
using ToDoListAppMVC.Utilities;

[AuthorizeUser]
public class ToDoController : Controller
{
    private readonly IToDoService _toDoService;
    private readonly ExcelExportService _excelExportService;

    public ToDoController(IToDoService toDoService, ExcelExportService excelExportService)
    {
        _toDoService = toDoService;
        _excelExportService = excelExportService;
    }

    private string GetCurrentUser() => HttpContext.Session.GetString("Email");
    private string GetCurrentUserRole() => HttpContext.Session.GetString("Role");

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        try
        {
            Logger.LogInfo("Fetching ToDo list", "ToDoController");

            string currentUser = GetCurrentUser();
            string userRole = GetCurrentUserRole();
            string searchQuery = Request.Query["search"];

            var items = _toDoService.GetAll(currentUser, userRole).ToList();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                Logger.LogInfo($"Filtering ToDo list with search query: {searchQuery}", "ToDoController");
                items = items.Where(x => x.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            var totalItems = items.Count;
            var paginatedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["SearchQuery"] = searchQuery;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);

            Logger.LogInfo($"Displaying page {page} of ToDo list", "ToDoController");
            return View(paginatedItems);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error fetching ToDo list", ex, "ToDoController");
            return View("Error");
        }
    }

    [HttpPost]
    public IActionResult Create(ToDoItem item)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Logger.LogInfo($"Attempting to create ToDo: {item.Title}", "ToDoController");
                _toDoService.Add(item, GetCurrentUser());
                Logger.LogInfo($"ToDo created successfully: {item.Title}", "ToDoController");
                return Ok();
            }

            Logger.LogWarning("Create ToDo failed: Invalid model", "ToDoController");
            return BadRequest();
        }
        catch (Exception ex)
        {
            Logger.LogError("Error creating ToDo", ex, "ToDoController");
            return BadRequest();
        }
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        try
        {
            var todo = _toDoService.GetById(id, GetCurrentUser(), GetCurrentUserRole());
            if (todo == null)
            {
                Logger.LogWarning($"ToDo with ID {id} not found", "ToDoController");
                return NotFound();
            }

            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                ? Json(new { id = todo.Id, title = todo.Title, isCompleted = todo.IsCompleted })
                : View(todo);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error fetching ToDo details", ex, "ToDoController");
            return View("Error");
        }
    }

    // AJAX-based Edit (GET)
    [HttpGet]
    public IActionResult Edit(int id)
    {
        try
        {
            var todo = _toDoService.GetById(id, GetCurrentUser(), GetCurrentUserRole());
            if (todo == null)
            {
                Logger.LogWarning($"ToDo with ID {id} not found", "ToDoController");
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Logger.LogInfo($"Returning ToDo edit form for ID {id} via AJAX", "ToDoController");
                return Json(new { id = todo.Id, title = todo.Title, isCompleted = todo.IsCompleted });
            }

            return View(todo);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error fetching ToDo for editing", ex, "ToDoController");
            return View("Error");
        }
    }

    // AJAX-based Edit (POST)
    [HttpPost]
    public IActionResult Edit(int id, [FromBody] ToDoItem toDoItem)
    {
        try
        {
            if (id != toDoItem.Id || toDoItem == null)
            {
                Logger.LogWarning($"Edit failed: ID mismatch or invalid ToDo item", "ToDoController");
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                Logger.LogInfo($"Attempting to update ToDo: {toDoItem.Title}", "ToDoController");
                _toDoService.Update(toDoItem, GetCurrentUser(), GetCurrentUserRole());
                Logger.LogInfo($"ToDo updated successfully: {toDoItem.Title}", "ToDoController");
                return Ok();
            }

            Logger.LogWarning("Edit failed: Invalid model", "ToDoController");
            return BadRequest();
        }
        catch (Exception ex)
        {
            Logger.LogError("Error updating ToDo", ex, "ToDoController");
            return BadRequest();
        }
    }


    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            _toDoService.Delete(id, GetCurrentUser(), GetCurrentUserRole());
            Logger.LogInfo($"ToDo with ID {id} deleted successfully", "ToDoController");
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            Logger.LogWarning($"Unauthorized ToDo delete attempt for ID: {id}", "ToDoController");
            return Forbid();
        }
        catch (Exception ex)
        {
            Logger.LogError("Error deleting ToDo", ex, "ToDoController");
            return BadRequest();
        }
    }

    [HttpGet]
    public IActionResult ExportToExcel(string searchQuery = "")
    {
        try
        {
            Logger.LogInfo($"Exporting ToDo list to Excel with search query: {searchQuery}", "ToDoController");

            var items = _toDoService.GetAll(GetCurrentUser(), GetCurrentUserRole()).ToList();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                items = items.Where(x => x.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            var excelFile = _excelExportService.ExportToExcel(items);
            Logger.LogInfo("Exported ToDo list to Excel successfully", "ToDoController");

            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ToDoList.xlsx");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error exporting ToDo list to Excel", ex, "ToDoController");
            return View("Error");
        }
    }
}
