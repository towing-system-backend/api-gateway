using Application.Core;

namespace Auth.Application
{
    public class AccountNotFoundError : ApplicationError
    {
        public AccountNotFoundError() : base("Account not found.") { }
    }
}