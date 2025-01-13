using Application.Core;
using RabbitMQ.Contracts;

namespace Auth.Application
{
    public class SignUpCommandHandler(IdService<string> idService, IPasswordService passwordService, ICryptoService cryptoService, IEmailService<EmailInfo> emailService, IMessageBrokerService messageBrokerService, IAccountRepository accountRepository) : IService<SignUpCommand, SignUpResponse>
    {
        private readonly IdService<string> _idService = idService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IEmailService<EmailInfo> _emailService = emailService;
        private readonly IMessageBrokerService _messageBrokerService = messageBrokerService;
        private readonly IAccountRepository _accountRepository = accountRepository;

        public async Task<Result<SignUpResponse>> Execute(SignUpCommand command)
        {
            var account = await _accountRepository.FindByEmail(command.Email);
            if (account.HasValue()) return Result<SignUpResponse>.MakeError(new UserAlreadyExistsError());

            var id = _idService.GenerateId();
            var password = _passwordService.GeneratePassword();
            var hashedPassword = _cryptoService.Hash(password);
            await _emailService.SendEmail(new EmailInfo(command.Email, "Welcome Email", "Your password is: " + password));
            await _accountRepository.Save(
                new Account(
                    id,
                    command.SupplierCompanyId,
                    null,
                    command.Email,
                    command.Role,
                    hashedPassword,
                    DateTime.UtcNow.AddDays(1)
                )
            );

            await _messageBrokerService.Publish(
                new CreateUser(
                    id,
                    command.SupplierCompanyId,
                    command.Name,
                    command.Image,
                    command.Email,
                    command.Role,
                    command.Status,
                    command.PhoneNumber,
                    command.IdentificationNumber
                )
            );

            return Result<SignUpResponse>.MakeSuccess(new SignUpResponse(id));
        }
    }
}