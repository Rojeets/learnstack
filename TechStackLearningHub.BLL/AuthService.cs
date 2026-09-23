using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using TechStackLearningHub.DAL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.BLL
{
    // Login outcome. Deactivated is a distinct state so the calling page can
    // show an accurate message instead of "invalid credentials".
    public class LoginResult
    {
        public bool Success { get; set; }
        public bool Deactivated { get; set; }
        public User User { get; set; }

        public static LoginResult Failure()
        {
            return new LoginResult { Success = false, Deactivated = false };
        }

        public static LoginResult AccountDeactivated()
        {
            return new LoginResult { Success = false, Deactivated = true };
        }

        public static LoginResult Succeeded(User user)
        {
            return new LoginResult { Success = true, User = user };
        }
    }

    public class AuthService
    {
        // PBKDF2-SHA256 constants. These are FROZEN and identical to the values
        // documented in database/TechStackLearningHub.sql so that the seeded
        // admin/student accounts validate against this exact derivation.
        public const int PBKDF2Iterations = 100000;
        public const int SaltSizeBytes = 16;
        public const int HashSizeBytes = 32;

        private readonly UserRepository _userRepository = new UserRepository();

        public byte[] GenerateSalt()
        {
            var salt = new byte[SaltSizeBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public byte[] DeriveHash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PBKDF2Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSizeBytes);
            }
        }

        public bool Register(string username, string email, string password)
        {
            // Fail fast: reject duplicates before spending any effort on hashing.
            if (_userRepository.GetUserByUsername(username) != null)
                throw new InvalidOperationException("That username is already taken.");

            byte[] salt = GenerateSalt();
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = Convert.ToBase64String(DeriveHash(password, salt)),
                PasswordSalt = Convert.ToBase64String(salt),
                // Registration can never self-assign a role: always Student.
                RoleID = _userRepository.GetRoleIdByName("Student"),
                IsActive = true
            };
            if (user.RoleID <= 0)
                throw new InvalidOperationException("The Student role is not configured.");

            _userRepository.InsertUser(user);
            return true;
        }

        public LoginResult Login(string username, string password)
        {
            User user = _userRepository.GetUserByUsername(username);
            if (user == null)
                return LoginResult.Failure();

            if (!user.IsActive)
                return LoginResult.AccountDeactivated();

            byte[] salt = Convert.FromBase64String(user.PasswordSalt);
            byte[] expected = Convert.FromBase64String(user.PasswordHash);
            byte[] actual = DeriveHash(password, salt);

            // Constant-time comparison — equivalent of CryptographicOperations
            // .FixedTimeEquals, which is not available in .NET Framework 4.8.
            if (!FixedTimeEquals(actual, expected))
                return LoginResult.Failure();

            user.RoleName = _userRepository.GetRoleNameById(user.RoleID);
            user.PasswordHash = null;
            user.PasswordSalt = null;
            return LoginResult.Succeeded(user);
        }

        public void CreateSessionForUser(User user)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
                throw new InvalidOperationException("No HTTP session is available.");

            context.Session["UserID"] = user.UserID;
            context.Session["Username"] = user.Username;
            context.Session["RoleName"] = user.RoleName ?? "Student";

            // Cookie enables web.config <authorization> rules; Session backs
            // fast per-page lookups. Both are set so both gatekeepers work.
            FormsAuthentication.SetAuthCookie(user.Username, false);
        }

        public void Logout()
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
                return;

            context.Session.Clear();
            context.Session.Abandon();
            FormsAuthentication.SignOut();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] - right[i];
            }
            return diff == 0;
        }
    }
}