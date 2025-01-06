﻿using Application.Core;
using Auth.Application;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Infrastructure
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IdService<string> idService, Logger logger, ITokenService<string> tokenService, ICryptoService cryptoService, IPasswordService passwordService, IEmailService<EmailInfo> emailInfo, IMessageBrokerService messageBrokerService, IAccountRepository accountRepository, IPerformanceLogsRepository performanceLogsRepository) : ControllerBase
    {
        private readonly IdService<string> _idService = idService;
        private readonly Logger _logger = logger;
        private readonly ITokenService<string> _tokenService = tokenService;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IEmailService<EmailInfo> _emailService = emailInfo;
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

        [HttpPost("signup")]
        public async Task<ObjectResult> SignUp([FromBody] SignUpDto signUpDto)
        {
            var command = new SignUpCommand(
                signUpDto.SupplierCompanyId,
                signUpDto.Name,
                signUpDto.Image,
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
    }
}