using Application.Core;

namespace Auth.Application
{
    public class ResetPasswordCommandHandler(ICryptoService cryptoService, IAccountRepository accountRepository) : IService<ResetPasswordCommand, ResetPasswordResponse>
    {
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IAccountRepository _accountRepository = accountRepository;
        public async Task<Result<ResetPasswordResponse>> Execute(ResetPasswordCommand command)
        {
            var account = await _accountRepository.FindByEmail(command.Email);
            if (!account.HasValue()) return Result<ResetPasswordResponse>.MakeError(new AccountNotFoundError());

            if (command.NewPassword != command.NewPasswordConfirmation) return Result<ResetPasswordResponse>.MakeError(new PasswordsDoNotMatchError());

            var credentials = account.Unwrap();

            var updatedAccount = new Account(credentials.UserId, credentials.Email, _cryptoService.Hash(command.NewPassword), null);

            await _accountRepository.Save(updatedAccount);

            return Result<ResetPasswordResponse>.MakeSuccess(new ResetPasswordResponse(credentials.UserId));
        }
    }
}