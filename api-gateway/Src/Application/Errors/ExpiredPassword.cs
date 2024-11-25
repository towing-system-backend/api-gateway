using Application.Core;

namespace Auth.Application
{
    public class ExpiredPasswordError : ApplicationError
    {
        public ExpiredPasswordError() : base("Expired password.") { }
    }
}