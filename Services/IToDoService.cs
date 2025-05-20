using ToDoListAppMVC.Models;

namespace ToDoListAppMVC.Services
{
    public interface IToDoService
    {
        IEnumerable<ToDoItem> GetAll(string currentUser, string userRole);
        ToDoItem GetById(int id, string currentUser, string userRole);
        void Add(ToDoItem item, string currentUser);
        void Update(ToDoItem item, string currentUser, string userRole);
        void Delete(int id, string currentUser, string userRole);
    }
}
