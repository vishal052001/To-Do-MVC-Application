namespace ToDoListAppMVC.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
