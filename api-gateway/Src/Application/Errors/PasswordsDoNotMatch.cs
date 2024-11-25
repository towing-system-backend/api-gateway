using Application.Core;

namespace Auth.Application
{
    public class PasswordsDoNotMatchError : ApplicationError
    {
        public PasswordsDoNotMatchError() : base("Passwords do not match.") { }
    }
}