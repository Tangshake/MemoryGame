namespace RegisterService.PasswordManagement;

public interface IPasswordHash
{
    /// <summary>
    /// Hash provided text
    /// </summary>
    /// <param name="password">Password</param>
    /// <param name="workFactor">Level of iterations performed in order to hash the password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password, int workFactor);

    /// <summary>
    /// Verify that provided password equals hashed one.
    /// </summary>
    /// <param name="password">Password to check against hashed one</param>
    /// <param name="hashedPassword">Hashed password</param>
    /// <returns>True if password matches hashed one</returns>
    bool VerifyHashedPassword(string password, string hashedPassword);
}
