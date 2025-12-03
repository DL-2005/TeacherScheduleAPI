using System.Security.Cryptography;
using System.Text;

namespace TeacherScheduleAPI.Services
{
    public class PasswordService
    {
        public static string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("ERROR: Password is null or empty!");
                    return string.Empty;
                }

                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(password);
                    var hash = sha256.ComputeHash(bytes);
                    var result = Convert.ToBase64String(hash);

                    Console.WriteLine($"HashPassword Input: {password}");
                    Console.WriteLine($"HashPassword Output: {result}");

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in HashPassword: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }

        public static bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(hashedPassword))
                {
                    Console.WriteLine("ERROR: Input or hashed password is null!");
                    return false;
                }

                var inputHash = HashPassword(inputPassword);

                Console.WriteLine($"VerifyPassword - Input: {inputPassword}");
                Console.WriteLine($"VerifyPassword - Generated Hash: {inputHash}");
                Console.WriteLine($"VerifyPassword - Expected Hash: {hashedPassword}");
                Console.WriteLine($"VerifyPassword - Match: {inputHash == hashedPassword}");

                return string.Equals(inputHash, hashedPassword, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in VerifyPassword: {ex.Message}");
                return false;
            }
        }
    }
}