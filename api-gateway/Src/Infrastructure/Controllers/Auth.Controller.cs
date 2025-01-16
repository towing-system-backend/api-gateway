using Application.Core;
using Auth.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Infrastructure
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IdService<string> idService, Logger logger, ITokenService<string> tokenService, ICryptoService cryptoService, IPasswordService passwordService, IEmailService<EmailInfo> emailInfo, IMessageBrokerService messageBrokerService, IAccountRepository accountRepository, IPerformanceLogsRepository performanceLogsRepository, IImagesCloudService<string> imagesService) : ControllerBase
    {
        private readonly IdService<string> _idService = idService;
        private readonly Logger _logger = logger;
        private readonly ITokenService<string> _tokenService = tokenService;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IEmailService<EmailInfo> _emailService = emailInfo;
        private readonly IImagesCloudService<string> _imagesService = imagesService;
        private readonly IMessageBrokerService _messageBrokerService = messageBrokerService;
        private readonly IAccountRepository _accountRepository = accountRepository;
        private readonly IPerformanceLogsRepository _performanceLogsRepository = performanceLogsRepository;

        [HttpPost("login")]
        public async Task<ObjectResult> Login([FromBody] LoginDto loginDto)
        {
            var command = new LoginCommand(loginDto.Email, loginDto.Password, loginDto.DeviceId);
            var handler =
                new ExceptionCatcher<LoginCommand, LoginResponse>(
                    new PerfomanceMonitor<LoginCommand, LoginResponse>(
                        new LoggingAspect<LoginCommand, LoginResponse>(
                            new LoginCommandHandler(_tokenService, _cryptoService, _accountRepository), _logger
                        ), _logger, _performanceLogsRepository, nameof(LoginCommandHandler), "Write"
                    ), ExceptionParser.Parse
                );   
            
            var res = await handler.Execute(command);

            return Ok(res.Unwrap());
        }

        [HttpPost("register/towdriver")]
        [Authorize(Roles = "Admin, Provider")]
        public async Task<ObjectResult> RegisterTowDriver([FromBody] CreateTowDriverDto createTowDriverDto)
        {
            var command = new RegisterTowDriverCommand(
                createTowDriverDto.SupplierCompanyId,
                createTowDriverDto.Name,
                createTowDriverDto.Email,
                createTowDriverDto.LicenseOwnerName,
                createTowDriverDto.LicenseIssueDate,
                createTowDriverDto.LicenseExpirationDate,
                createTowDriverDto.MedicalCertificateOwnerName,
                createTowDriverDto.MedicalCertificateAge,
                createTowDriverDto.MedicalCertificateIssueDate,
                createTowDriverDto.MedicalCertificateExpirationDate,
                createTowDriverDto.IdentificationNumber
            );
            var handler =
                new ExceptionCatcher<RegisterTowDriverCommand, RegisterTowDriverResponse>(
                    new PerfomanceMonitor<RegisterTowDriverCommand, RegisterTowDriverResponse>(
                        new LoggingAspect<RegisterTowDriverCommand, RegisterTowDriverResponse>(
                            new RegisterTowDriverCommandHandler(_idService, _passwordService, _cryptoService, _emailService, _messageBrokerService, _accountRepository), _logger
                        ), _logger, _performanceLogsRepository, nameof(RegisterTowDriverCommandHandler), "Write"
                    ), ExceptionParser.Parse
                );
            var res = await handler.Execute(command);

            return Ok(res.Unwrap());
        }

        [HttpPost("signup")]
        [Authorize(Roles = "Admin, Provider")]
        [Consumes("multipart/form-data")]
        public async Task<ObjectResult> SignUp([FromForm] SignUpDto signUpDto)
        {
            var image = await _imagesService.UploadImage(signUpDto.Image);
            var command = new SignUpCommand(
                signUpDto.SupplierCompanyId,
                signUpDto.Name,
                image,
                signUpDto.Email,
                signUpDto.Role,
                signUpDto.Status,
                signUpDto.PhoneNumber,
                signUpDto.IdentificationNumber
            );
            var handler =
                new ExceptionCatcher<SignUpCommand, SignUpResponse>(
                    new PerfomanceMonitor<SignUpCommand, SignUpResponse>(
                        new LoggingAspect<SignUpCommand, SignUpResponse>(
                            new SignUpCommandHandler(_idService, _passwordService, _cryptoService, _emailService, _messageBrokerService, _accountRepository), _logger
                        ), _logger, _performanceLogsRepository, nameof(SignUpCommandHandler), "Write"
                    ), ExceptionParser.Parse
                );
            var res = await handler.Execute(command);

            return Ok(res.Unwrap());
        }

        [HttpPost("reset-password")]
        public async Task<ObjectResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var command = new ResetPasswordCommand(
                resetPasswordDto.Email,
                resetPasswordDto.NewPassword,
                resetPasswordDto.NewPasswordConfirmation
            );
            
            var handler =
                new ExceptionCatcher<ResetPasswordCommand, ResetPasswordResponse>(
                    new PerfomanceMonitor<ResetPasswordCommand, ResetPasswordResponse>(
                        new LoggingAspect<ResetPasswordCommand, ResetPasswordResponse>(
                            new ResetPasswordCommandHandler(_cryptoService, _accountRepository), _logger
                        ), _logger, _performanceLogsRepository, nameof(ResetPasswordCommandHandler), "Write"
                    ), ExceptionParser.Parse
                );
            var res = await handler.Execute(command);

            return Ok(res.Unwrap());
        }
    }
}