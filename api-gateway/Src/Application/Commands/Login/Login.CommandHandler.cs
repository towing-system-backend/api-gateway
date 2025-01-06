using Application.Core;

namespace Auth.Application
{
    public class LoginCommandHandler(ITokenService<string> tokenService, ICryptoService cryptoService, IAccountRepository accountRepository) : IService<LoginCommand, LoginResponse>
    {
        private readonly ITokenService<string> _tokenService = tokenService;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IAccountRepository _accountRepository = accountRepository;

        public async Task<Result<LoginResponse>> Execute(LoginCommand command)
        {
            var account = await _accountRepository.FindByEmail(command.Email);
            if (!account.HasValue()) return Result<LoginResponse>.MakeError(new AccountNotFoundError());

            var credentials = account.Unwrap();
            if (credentials.PasswordExpirationDate != null & DateTime.UtcNow > credentials.PasswordExpirationDate)
                return Result<LoginResponse>.MakeError(new ExpiredPasswordError());

            if (!_cryptoService.Verify(command.Password, credentials.Password))
                return Result<LoginResponse>.MakeError(new InvalidPasswordError());

            var token = _tokenService.GenerateToken(credentials.UserId, credentials.Role);

            if (command.DeviceId != null)
                await _accountRepository.Save(
                    new Account(
                        credentials.UserId,
                        command.DeviceId,
                        credentials.Email,
                        credentials.Role,
                        credentials.Password,
                        credentials.PasswordExpirationDate
                    )
                );

            return Result<LoginResponse>.MakeSuccess(new LoginResponse(token));
        }
    }
}