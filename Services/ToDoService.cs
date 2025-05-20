using ToDoListAppMVC.Models;
using ToDoListAppMVC.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ToDoListAppMVC.Services
{
    public class ToDoService : IToDoService
    {
        private readonly ToDoContext _context;

        public ToDoService(ToDoContext context)
        {
            _context = context;
        }

        public IEnumerable<ToDoItem> GetAll(string currentUser, string userRole)
        {
            try
            {
                Logger.LogInfo($"Fetching ToDo items for user: {currentUser}", "ToDoService");

                // Admins can see all ToDos, others can see only their own
                var items = userRole == "Admin"
                    ? _context.ToDoItems.ToList()
                    : _context.ToDoItems.Where(t => t.CreatedBy == currentUser).ToList();

                Logger.LogInfo($"Fetched {items.Count} items", "ToDoService");
                return items;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error fetching ToDo items", ex, "ToDoService");
                throw;
            }
        }

        public ToDoItem GetById(int id, string currentUser, string userRole)
        {
            try
            {
                Logger.LogInfo($"Fetching ToDo item with ID: {id}", "ToDoService");

                var item = _context.ToDoItems.FirstOrDefault(t => t.Id == id);

                // Ensure StandardUser can only access their own items
                if (item == null || (userRole != "Admin" && item.CreatedBy != currentUser))
                {
                    Logger.LogWarning($"Unauthorized access attempt for ToDo ID: {id}", "ToDoService");
                    return null;
                }

                return item;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error fetching ToDo item with ID: {id}", ex, "ToDoService");
                throw;
            }
        }

        public void Add(ToDoItem item, string currentUser)
        {
            try
            {
                item.CreatedBy = currentUser;
                _context.ToDoItems.Add(item);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error adding ToDo item", ex, "ToDoService");
                throw;
            }
        }

        public void Update(ToDoItem item, string currentUser, string userRole)
        {
            try
            {
                var existingItem = GetById(item.Id, currentUser, userRole);
                if (existingItem == null) throw new UnauthorizedAccessException("Not allowed");

                existingItem.Title = item.Title;
                existingItem.IsCompleted = item.IsCompleted;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error updating ToDo item", ex, "ToDoService");
                throw;
            }
        }

        public void Delete(int id, string currentUser, string userRole)
        {
            try
            {
                var item = GetById(id, currentUser, userRole);
                if (item == null) throw new UnauthorizedAccessException("Not allowed");

                _context.ToDoItems.Remove(item);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error deleting ToDo item", ex, "ToDoService");
                throw;
            }
        }
    }
}
