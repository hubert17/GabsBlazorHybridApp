using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GabsHybridApp.Shared.Services
{
    public class UserService
    {
        private readonly IDbContextFactory<HybridAppDbContext> _factory;
        private readonly HybridAppDbContext _db;

        public UserService(IDbContextFactory<HybridAppDbContext> factory)
        {
            _factory = factory;
            _db = _factory.CreateDbContext();
        }

        public UserAccount? Authenticate(string username, string password)
        {
            CreateAdmin(); // Comment out this line if you already have admin account

            if (username.Contains("@"))
                if (username.Split('@')[0].Equals(UserAccount.DEFAULT_ADMIN_LOGIN, StringComparison.OrdinalIgnoreCase))
                    username = UserAccount.DEFAULT_ADMIN_LOGIN;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = GetSingleUser(username);
            if (user == null)
                return null;
            if (!user.IsActive)
                return null;

            bool valid = VerifyPasswordHash(password, user.PasswordSalt, user.PasswordHash);
            if (valid)
            {
                user.LastLogin = DateTime.Now;
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                user.PasswordHash = null;
                user.PasswordSalt = null;
                return user;
            }

            return null;
        }

        public Guid? Create(string? username, string? password, string? roles = "", bool requiresActivation = false)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            username = username.Trim();
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.@]*$"))
                return null;

            var user = new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = username.Trim().ToLower()
            };

            var userExists = _db.UserAccounts.FirstOrDefault(x => x.Username!.ToLower() == user.Username.ToLower()) != null;
            if (userExists)
                return null;

            // Create PasswordHash
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                user.PasswordSalt = hmac.Key;
                user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }

            user.Roles = Regex.Replace(roles!, @"\s+", "");
            user.CreatedOn = DateTime.Now;
            user.IsActive = !requiresActivation;

            _db.UserAccounts.Add(user);
            _db.SaveChanges();

            return user.Id;
        }

        public bool ChangePassword(string? username, string password = "", string? newPassword = "", bool forceChange = false)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
                return false;

            if (forceChange == false && string.IsNullOrWhiteSpace(password))
                return false;

            var user = GetSingleUser(username);
            if (user == null)
                return false;

            var validPassword = forceChange || VerifyPasswordHash(password, user.PasswordSalt, user.PasswordHash);
            if (validPassword)
            {
                // Overwrite with new PasswordHash
                using (var hmac = new System.Security.Cryptography.HMACSHA512())
                {
                    user.PasswordSalt = hmac.Key;
                    user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));
                }

                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return true;
            }
            else
                return false;

        }

        private void CreateAdmin()
        {
            var hasAdmin = _db.UserAccounts.FirstOrDefault(x => x.Roles == UserAccount.DEFAULT_ADMIN_ROLENAME) != null;
            if (!hasAdmin)
            {
                Create(UserAccount.DEFAULT_ADMIN_LOGIN, UserAccount.DEFAULT_ADMIN_LOGIN, UserAccount.DEFAULT_ADMIN_ROLENAME);
            }
        }

        private UserAccount? GetSingleUser(string username)
        {
            return _db.UserAccounts.SingleOrDefault(x => x.Username!.ToLower() == username.ToLower());
        }

        private bool VerifyPasswordHash(string userPassword, byte[]? passwordSalt, byte[]? passwordHash)
        {
            if (passwordSalt == null || passwordHash == null) return false;
            // Verify PasswordHash
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userPassword));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                        return false;
                }
            }

            return true;
        }

        public string SetActivation(string username, bool isActive)
        {
            var user = GetSingleUser(username);
            if (user != null)
            {
                user.IsActive = isActive;
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return "user is " + (user!.IsActive ? "active" : "inactive");
            }

            return "user not found";
        }

        public bool AssignRoles(string username, string roles = "")
        {
            var user = GetSingleUser(username);
            if (user != null)
            {
                roles = Regex.Replace(roles!, @"\s+", "");
                var arrRoles = user.Roles!.Split(',').Concat(roles.Split(',')).Distinct();
                user.Roles = string.Join(",", arrRoles);
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return true;
            }

            return false;
        }

        public bool RemoveRoles(string username, string roles = "")
        {
            var user = GetSingleUser(username);
            if (user != null)
            {
                roles = Regex.Replace(roles!, @"\s+", "");
                var arrRoles = user.Roles!.Split(',');
                arrRoles = arrRoles.Where(x => !roles.Split(',').Contains(x)).ToArray();
                user.Roles = string.Join(",", arrRoles);
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return true;
            }

            return false;
        }
    }
}

namespace GabsHybridApp.Shared.Models
{
    public class UserAccount
    {
        // Change this to your desired default admin login, password and role name.
        public const string DEFAULT_ADMIN_LOGIN = "admin";
        public const string DEFAULT_ADMIN_ROLENAME = "administrator";

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        [StringLength(50)]
        public string? Username { get; set; }
        [JsonIgnore]
        public byte[]? PasswordHash { get; set; }
        [JsonIgnore]
        public byte[]? PasswordSalt { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public string? Roles { get; set; } // comma-separated
    }
}

