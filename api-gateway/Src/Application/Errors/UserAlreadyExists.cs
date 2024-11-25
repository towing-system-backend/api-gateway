using Application.Core;

namespace Auth.Application
{
    public class UserAlreadyExistsError : ApplicationError
    {
        public UserAlreadyExistsError() : base("User already exists.") { }
    }
}