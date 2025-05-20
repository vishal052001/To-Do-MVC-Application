namespace ToDoListAppMVC.Models
{
    public class User
    {
        public string FirstName {  get; set; } 
        public string LastName { get; set; }
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } = "StandardUser";
        public int? PromotedByAdminId { get; set; }
    }
}
