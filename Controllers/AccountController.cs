using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListAppMVC.Models;
using ToDoListAppMVC.Services;
using ToDoListAppMVC.Utilities;

namespace ToDoListAppMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly ToDoContext _context;

        public AccountController(AuthService authService, ToDoContext context)
        {
            _authService = authService;
            _context = context;
        }

        public IActionResult Login()
        {
            try
            {
                Logger.LogInfo("Accessed Login view", "Account");
                return View();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to render Login view", ex, "Account");
                return View("Error");
            }
        }

        public IActionResult Signup()
        {
            try
            {
                Logger.LogInfo("Accessed Signup view", "Account");
                return View();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to render Signup view", ex, "Account");
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            try
            {
                Logger.LogInfo($"Attempting login for email: {email}", "Account");
                var user = _authService.Login(email, password);

                if (user != null)
                {
                    Logger.LogInfo($"Login successful for user: {user.Username}", "Account");

                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("Role", user.Role);

                    return RedirectToAction("Index", "ToDo");
                }

                Logger.LogWarning($"Login failed: Invalid email or password for email: {email}", "Account");
                ViewBag.Error = "Invalid email or password.";
                return View();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during login for email: {email}", ex, "Account");
                ViewBag.Error = "An error occurred during login.";
                return View();
            }
        }

        [HttpPost]
        public IActionResult Signup(User user)
        {
            try
            {
                Logger.LogInfo($"Attempting signup for user: {user.Username}", "Account");

                user.Role = "StandardUser";

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    foreach (var error in errors)
                    {
                        Logger.LogWarning($"ModelState error: {error}", "Account");
                    }

                    ViewBag.ErrorMessage = "Please fill out all fields correctly.";
                    return View(user);
                }

                if (_authService.Register(user))
                {
                    Logger.LogInfo($"Signup successful for user: {user.Username}", "Account");
                    ViewBag.SuccessMessage = "Signup successful! Please login.";
                    return View();
                }

                Logger.LogWarning("Signup failed: Username already exists.", "Account");
                ViewBag.ErrorMessage = "Username or email already exists.";
                return View();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during signup for user: {user.Username}", ex, "Account");
                ViewBag.ErrorMessage = "An error occurred during signup. Please try again later.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                Logger.LogInfo($"User logged out: {username}", "Account");

                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during logout", ex, "Account");
                return RedirectToAction("Login");
            }
        }

        public IActionResult Profile()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    Logger.LogWarning("Profile access denied: User not logged in.", "Account");
                    return RedirectToAction("Login");
                }

                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
                if (user == null)
                {
                    Logger.LogWarning($"Profile access denied: User not found for ID {userId}.", "Account");
                    return RedirectToAction("Login");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during profile access", ex, "Account");
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public IActionResult EditProfile(User updatedUser)
        {
            string userId = HttpContext.Session.GetString("UserId");

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    Logger.LogWarning("Profile edit denied: User not logged in.", "Account");
                    return RedirectToAction("Login");
                }

                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
                if (user == null)
                {
                    Logger.LogWarning($"Profile edit failed: User not found for ID {userId}.", "Account");
                    return RedirectToAction("Login");
                }

                // Update only editable fields for StandardUsers
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Username = updatedUser.Username;
                user.Email = updatedUser.Email;
                user.Phone = updatedUser.Phone;

                if (!string.IsNullOrEmpty(updatedUser.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
                }

                // Allow role change only for Admins
                var currentRole = HttpContext.Session.GetString("Role");
                if (currentRole == "Admin")
                {
                    user.Role = updatedUser.Role;
                }

                _context.Users.Update(user);
                _context.SaveChanges();

                Logger.LogInfo($"Profile updated successfully for user ID: {userId}", "Account");
                ViewBag.Success = "Profile updated successfully.";
                return RedirectToAction("Index", "ToDo");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during profile edit for user ID: {userId}", ex, "Account");
                ViewBag.Error = "An error occurred while updating the profile.";
                return RedirectToAction("Index", "ToDo");
            }
        }

        public IActionResult ManageUsers()
        {
            try
            {
                var currentRole = HttpContext.Session.GetString("Role");
                if (currentRole != "Admin")
                {
                    Logger.LogWarning("Access denied to ManageUsers: User is not an admin.", "Account");
                    return RedirectToAction("Login", "Account");
                }

                var users = _context.Users.ToList();
                Logger.LogInfo($"Admin accessed ManageUsers view.", "Account");
                return View(users);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during ManageUsers access", ex, "Account");
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult ChangeUserRole(int userId, string newRole)
        {
            try
            {
                // Check if the current user is an admin
                var currentRole = HttpContext.Session.GetString("Role");
                var currentUserId = HttpContext.Session.GetString("UserId");

                if (currentRole != "Admin")
                {
                    Logger.LogWarning("Access denied to ChangeUserRole: User is not an admin.", "Account");
                    TempData["Error"] = "You do not have permission to change roles.";
                    return RedirectToAction("ManageUsers");
                }

                // Fetch the user being updated
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    Logger.LogWarning($"User not found with ID: {userId}", "Account");
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageUsers");
                }

                // Check demotion logic: Prevent demoting an admin not promoted by the current admin
                //if (newRole == "StandardUser" && user.Role == "Admin")
                //{
                //    if (user.PromotedByAdminId != null && user.PromotedByAdminId.ToString() != currentUserId)
                //    {
                //        Logger.LogWarning($"Permission denied: Admin {currentUserId} tried to demote Admin {userId} not promoted by them.", "Account");
                //        TempData["Error"] = "You can only demote admins you promoted.";
                //        return RedirectToAction("ManageUsers");
                //    }
                //}

                // Update the PromotedByAdminId when promoting a user to Admin
                if (newRole == "Admin" && user.Role == "StandardUser")
                {
                    user.PromotedByAdminId = int.Parse(currentUserId);
                }

                // Update the user's role
                user.Role = newRole;
                _context.Users.Update(user);
                _context.SaveChanges();

                Logger.LogInfo($"Role updated successfully for user ID: {userId} to {newRole}.", "Account");
                TempData["Success"] = $"User role updated to {newRole}.";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during ChangeUserRole for user ID: {userId}.", ex, "Account");
                TempData["Error"] = "An error occurred while changing the user role.";
                return RedirectToAction("ManageUsers");
            }
        }


    }
}
