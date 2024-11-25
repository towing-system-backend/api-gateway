namespace Application.Core
{
    public class PasswordGenerator : IPasswordService
    {
        private readonly Random _random = new();
        private readonly int _length = 7;

        public string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, _length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
