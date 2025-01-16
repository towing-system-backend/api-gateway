using Application.Core;
using RabbitMQ.Contracts;

namespace Auth.Application
{
    public enum Role{TowDriver}

    public class RegisterTowDriverCommandHandler(
        IdService<string> idService,
        IPasswordService passwordService,
        ICryptoService cryptoService,
        IEmailService<EmailInfo> emailService,
        IMessageBrokerService messageBrokerService,
        IAccountRepository accountRepository
    ) : IService<RegisterTowDriverCommand, RegisterTowDriverResponse>
    {
        private readonly IdService<string> _idService = idService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IEmailService<EmailInfo> _emailService = emailService;
        private readonly IMessageBrokerService _messageBrokerService = messageBrokerService;
        private readonly IAccountRepository _accountRepository = accountRepository;

        public async Task<Result<RegisterTowDriverResponse>> Execute(RegisterTowDriverCommand command)
        {
            var account = await _accountRepository.FindByEmail(command.Email);
            if (account.HasValue()) return Result<RegisterTowDriverResponse>.MakeError(new UserAlreadyExistsError());
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
                    Role.TowDriver.ToString(),
                    hashedPassword,
                    DateTime.UtcNow.AddDays(1)
                )
            );

            await _messageBrokerService.Publish(
                new CreateTowDriver(
                    id,
                    command.SupplierCompanyId,
                    command.Name,
                    command.Email,
                    command.LicenseOwnerName,
                    command.LicenseIssueDate,
                    command.LicenseExpirationDate,
                    command.MedicalCertificateOwnerName,
                    command.MedicalCertificateAge,
                    command.MedicalCertificateIssueDate,
                    command.MedicalCertificateExpirationDate,
                    command.IdentificationNumber
                )
            ); 

            return Result<RegisterTowDriverResponse>.MakeSuccess(new RegisterTowDriverResponse(id));
        }
    }
}