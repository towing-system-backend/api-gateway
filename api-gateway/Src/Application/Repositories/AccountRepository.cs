using Application.Core;

namespace Auth.Application
{
    public interface IAccountRepository
    {
        Task Save(Account account);
        Task<Optional<Account>> FindByEmail(string email);
        //Task<Optional<string>> Remove(string accountId);
    }
}