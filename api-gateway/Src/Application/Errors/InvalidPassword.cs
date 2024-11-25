using Application.Core;

namespace Auth.Application
{
    public class InvalidPasswordError : ApplicationError
    {
        public InvalidPasswordError() : base("Invalid password.") { }
    }
}