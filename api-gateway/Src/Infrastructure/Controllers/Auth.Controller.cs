using Application.Core;
using Auth.Application;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Infrastructure
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IdService<string> idService, ITokenService<string> tokenService, ICryptoService cryptoService, IPasswordService passwordService, IEmailService<EmailInfo> emailInfo, IAccountRepository accountRepository) : ControllerBase
    {
        private readonly IdService<string> _idService = idService;
        private readonly ITokenService<string> _tokenService = tokenService;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IEmailService<EmailInfo> _emailService = emailInfo;
        private readonly IAccountRepository _accountRepository = accountRepository;

        [HttpPost("login")]
        public async Task<ObjectResult> Login([FromBody] LoginDto loginDto)
        {
            var command = new LoginCommand(loginDto.Email, loginDto.Password);
            var handler =
                new ExceptionCatcher<LoginCommand, LoginResponse>(
                    new PerfomanceMonitor<LoginCommand, LoginResponse>(
                        new LoggingAspect<LoginCommand, LoginResponse>(
                            new LoginCommandHandler(_tokenService, _cryptoService, _accountRepository)
                        )
                    ), ExceptionParser.Parse
                );            
            var res = await handler.Execute(command);

            return Ok(res.Unwrap());
        }

        [HttpPost("signup")]
        public async Task<ObjectResult> SignUp([FromBody] SignUpDto signUpDto)
        {
            var command = new SignUpCommand(signUpDto.UserName, signUpDto.IdentificationNumber, signUpDto.Email);
            var handler =
                new ExceptionCatcher<SignUpCommand, SignUpResponse>(
                    new PerfomanceMonitor<SignUpCommand, SignUpResponse>(
                        new LoggingAspect<SignUpCommand, SignUpResponse>(
                            new SignUpCommandHandler(_idService, _passwordService, _cryptoService, _emailService, _accountRepository)
                        )
                    ), ExceptionParser.Parse
                );
            var res = await handler.Execute(command);

            return Ok(res.Unwrap());
        }
    }
}