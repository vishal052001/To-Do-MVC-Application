using Microsoft.AspNetCore.Http;
using ToDoListAppMVC.Models;
using ToDoListAppMVC.Utilities;
using System;
using System.Linq;

namespace ToDoListAppMVC.Services
{
    public class AuthService
    {
        private readonly ToDoContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ToDoContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Register(User user)
        {
            try
            {
                Logger.LogInfo($"Attempting to register user: {user.Username}", "AuthService");

                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    Logger.LogWarning($"Username '{user.Username}' is already taken.", "AuthService");
                    return false; // Username already exists
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Role = "StandardUser";
                _context.Users.Add(user);
                _context.SaveChanges();
                Logger.LogInfo($"User '{user.Username}' registered successfully.", "AuthService");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occurred during registration for user '{user.Username}'.", ex, "AuthService");
                throw;
            }
        }

        public User Login(string email, string password)
        {
            try
            {
                Logger.LogInfo($"Attempting login for email: {email}", "AuthService");

                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    Logger.LogInfo($"User '{user.Username}' logged in successfully.", "AuthService");

                    _httpContextAccessor.HttpContext.Session.SetString("Email", user.Email);
                    _httpContextAccessor.HttpContext.Session.SetString("Role", user.Role);

                    return user;
                }

                Logger.LogWarning($"Login failed for email: {email}. Incorrect email or password.", "AuthService");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occurred during login for email '{email}'.", ex, "AuthService");
                throw;
            }
        }

        public bool ChangeUserRole(int userId, string newRole)
        {
            try
            {
                Logger.LogInfo($"Attempting to change role for User ID: {userId} to '{newRole}'.", "AuthService");

                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    Logger.LogWarning($"User with ID '{userId}' not found.", "AuthService");
                    return false;
                }

                user.Role = newRole;
                _context.SaveChanges();
                Logger.LogInfo($"User ID '{userId}' role updated to '{newRole}'.", "AuthService");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occurred while changing role for User ID '{userId}'.", ex, "AuthService");
                throw;
            }
        }
    }
}
