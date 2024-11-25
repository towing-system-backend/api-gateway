namespace Auth.Application
{
    public class Account(string userId, string email, string password, DateTime? passwordExpirationDate)
    {
        public string UserId { get; private set; } = userId;
        public string Email { get; private set; } = email;
        public string Password { get; private set; } = password;
        public DateTime? PasswordExpirationDate { get; private set; } = passwordExpirationDate;
    }
}