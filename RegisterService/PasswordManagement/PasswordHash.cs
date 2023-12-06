namespace RegisterService.PasswordManagement
{
    public class PasswordHash : IPasswordHash
    {
        public string HashPassword(string password, int workFactor)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor);
        }

        public bool VerifyHashedPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
        }
    }
}
