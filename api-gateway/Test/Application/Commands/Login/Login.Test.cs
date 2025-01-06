using Application.Core;
using Moq;
using Auth.Application;
using Xunit;

namespace Auth.Test
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<ITokenService<string>> _tokenServiceMock;
        private readonly Mock<ICryptoService> _cryptoServiceMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly LoginCommandHandler _loginCommandHandler;

        public LoginCommandHandlerTests()
        {
            _tokenServiceMock = new Mock<ITokenService<string>>();
            _cryptoServiceMock = new Mock<ICryptoService>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _loginCommandHandler = new LoginCommandHandler(
                _tokenServiceMock.Object,
                _cryptoServiceMock.Object,
                _accountRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Should_Not_Login_When_Account_Not_Found()
        {
            // Arrange
            var command = new LoginCommand("andreshg@gmail.com", "Hola123", "4779c4ec-0d54-4731-8885-69122dd6db1c");

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Empty());

            // Act
            var result = await _loginCommandHandler.Execute(command);

            // Assert
            Assert.True(result.IsError);
            var exception = Assert.Throws<AccountNotFoundError>(() => result.Unwrap());
            Assert.Equal("Account not found.", exception.Message);

            _accountRepositoryMock.Verify(repo => repo.Save(It.IsAny<Account>()), Times.Never);
            _tokenServiceMock.Verify(service => service.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Not_Login_When_Password_Has_Expired()
        {
            // Arrange
            var command = new LoginCommand(
                "andreshg@gmail.com",
                "Hola123",
                "4779c4ec-0d54-4731-8885-69122dd6db1c"
            );

            var account = new Account(
                "07e24b54-4d06-479b-8249-9e98dffa7b28",
                "4779c4ec-0d54-4731-8885-69122dd6db1c",
                "andreshg@gmail.com",
                "TowDriver",
                "$2a$111$oDRvMB6ogg1$HRdae1nzRT57g0v/bM",
                DateTime.UtcNow.AddDays(-1)
            );

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Of(account));

            // Act
            var result = await _loginCommandHandler.Execute(command);

            // Assert
            Assert.True(result.IsError);
            var exception = Assert.Throws<ExpiredPasswordError>(() => result.Unwrap());
            Assert.Equal("Expired password.", exception.Message);

            _accountRepositoryMock.Verify(repo => repo.Save(It.IsAny<Account>()), Times.Never);
            _tokenServiceMock.Verify(service => service.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Not_Login_When_Password_Is_Invalid()
        {
            // Arrange
            var command = new LoginCommand(
                "andreshg@gmail.com",
                "Hola123",
                "4779c4ec-0d54-4731-8885-69122dd6db1c"
            );

            var account = new Account(
                "07e24b54-4d06-479b-8249-9e98dffa7b28",
                "4779c4ec-0d54-4731-8885-69122dd6db1c",
                "andreshg@gmail.com",
                "TowDriver",
                "$2a$111$oDRvMB6ogg1$HRdae1nzRT57g0v/bM",
                DateTime.UtcNow.AddYears(1)
            );
            
            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Of(account));

            _cryptoServiceMock.Setup(service => service.Verify(command.Password, account.Password))
                .Returns(false);

            // Act
            var result = await _loginCommandHandler.Execute(command);

            // Assert
            Assert.True(result.IsError);
            var exception = Assert.Throws<InvalidPasswordError>(() => result.Unwrap());
            Assert.Equal("Invalid password.", exception.Message);

            _accountRepositoryMock.Verify(repo => repo.Save(It.IsAny<Account>()), Times.Never);
            _tokenServiceMock.Verify(service => service.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Login_Successfully_With_Valid_Credentials()
        {
            // Arrange
            var command = new LoginCommand(
                "andreshg@gmail.com",
                "Hola123",
                "4779c4ec-0d54-4731-8885-69122dd6db1c"
            );

            var account = new Account(
                "07e24b54-4d06-479b-8249-9e98dffa7b28",
                "4779c4ec-0d54-4731-8885-69122dd6db1c",
                "andreshg@gmail.com",
                "TowDriver",
                "$2a$111$oDRvMB6ogg1$HRdae1nzRT57g0v/bM",
                DateTime.UtcNow.AddYears(1)
            );
            
            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Of(account));

            _cryptoServiceMock.Setup(service => service.Verify(command.Password, account.Password))
                .Returns(true);

            _tokenServiceMock.Setup(service => service.GenerateToken(account.UserId, account.Role))
                .Returns("648ea03b-2aaf-4a14-ac3b-6036f2c0d22a");

            // Act
            var result = await _loginCommandHandler.Execute(command);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal("648ea03b-2aaf-4a14-ac3b-6036f2c0d22a", result.Unwrap().Token);

            _accountRepositoryMock.Verify(repo => repo.Save(It.Is<Account>(acc =>
                    acc.UserId == account.UserId &&
                    acc.DeviceId == command.DeviceId &&
                    acc.Email == account.Email &&
                    acc.Role == account.Role &&
                    acc.Password == account.Password &&
                    acc.PasswordExpirationDate == account.PasswordExpirationDate
                )
            ), Times.Once);

            _tokenServiceMock.Verify(service => 
                service.GenerateToken(
                    account.UserId,
                    account.Role
                ), Times.Once);
        }

        [Fact]
        public async Task Should_Login_Successfully_Without_DeviceId()
        {
            // Arrange
            var command = new LoginCommand("user@example.com", "password", null);

            var account = new Account("user_id", "device_id", "user@example.com", "User", "hashed_password", DateTime.UtcNow.AddDays(1));

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Of(account));

            _cryptoServiceMock.Setup(service => service.Verify(command.Password, account.Password))
                .Returns(true);

            _tokenServiceMock.Setup(service => service.GenerateToken(account.UserId, account.Role))
                .Returns("648ea03b-2aaf-4a14-ac3b-6036f2c0d22a");

            // Act
            var result = await _loginCommandHandler.Execute(command);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal("648ea03b-2aaf-4a14-ac3b-6036f2c0d22a", result.Unwrap().Token);

            _accountRepositoryMock.Verify(repo => repo.Save(It.IsAny<Account>()), Times.Never);
            _tokenServiceMock.Verify(service => service.GenerateToken(account.UserId, account.Role), Times.Once);
        }
    }
}
