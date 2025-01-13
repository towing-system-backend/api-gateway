using Application.Core;
using Moq;
using Auth.Application;
using Xunit;

namespace Auth.Tests
{
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<ICryptoService> _cryptoServiceMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly ResetPasswordCommandHandler _resetPasswordCommandHandler;

        public ResetPasswordCommandHandlerTests()
        {
            _cryptoServiceMock = new Mock<ICryptoService>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _resetPasswordCommandHandler = new ResetPasswordCommandHandler(
                _cryptoServiceMock.Object,
                _accountRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Should_Not_Reset_Password_When_Account_Not_Found()
        {
            // Arrange
            var command = new ResetPasswordCommand(
                "andreshg@gmail.com",
                "Chao321",
                "Chao321"
            );

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Empty());

            // Act
            var result = await _resetPasswordCommandHandler.Execute(command);

            // Assert
            Assert.True(result.IsError);
            var exception = Assert.Throws<AccountNotFoundError>(() => result.Unwrap());
            Assert.Equal("Account not found.", exception.Message);

            _accountRepositoryMock.Verify(repo => repo.Save(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task Should_Not_Reset_Password_When_Passwords_Do_Not_Match()
        {
            // Arrange
            var command = new ResetPasswordCommand(
                "andreshg@gmail.com",
                "Chao321",
                "Cha321"
            );

            var account = new Account(
                "07e24b54-4d06-479b-8249-9e98dffa7b28",
                "00789dad-4d0c-493f-b84c-5560caea24a1",
                "4779c4ec-0d54-4731-8885-69122dd6db1c",
                "andreshg@gmail.com",
                "TowDriver",
                "$2a$111$oDRvMB6ogg1$HRdae1nzRT57g0v/bM",
                DateTime.UtcNow.AddYears(1)
            );

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Of(account));

            // Act
            var result = await _resetPasswordCommandHandler.Execute(command);

            // Assert
            Assert.True(result.IsError);
            var exception = Assert.Throws<PasswordsDoNotMatchError>(() => result.Unwrap());
            Assert.Equal("Passwords do not match.", exception.Message);

            _accountRepositoryMock.Verify(repo => repo.Save(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task Should_Reset_Password()
        {
            //Arrange
            var command = new ResetPasswordCommand(
                "andreshg@gmail.com",
                "Chao321",
                "Chao321"
            );

            var account = new Account(
                "07e24b54-4d06-479b-8249-9e98dffa7b28",
                "00789dad-4d0c-493f-b84c-5560caea24a1",
                "4779c4ec-0d54-4731-8885-69122dd6db1c",
                "andreshg@gmail.com",
                "TowDriver",
                "$2a$111$oDRvMB6ogg1$HRdae1nzRT57g0v/bM",
                DateTime.UtcNow.AddYears(1)
            );

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email)) .ReturnsAsync(Optional<Account>.Of(account));
            _cryptoServiceMock.Setup(service => service.Hash(command.NewPassword)) .Returns("sW$grujBqGMdK.kXN0MXClE"); 
            
            // Act
            var result = await _resetPasswordCommandHandler.Execute(command);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal("07e24b54-4d06-479b-8249-9e98dffa7b28", result.Unwrap().UserId);

            _accountRepositoryMock.Verify(repo => 
                repo.Save(It.Is<Account>(acc => 
                    acc.UserId == account.UserId &&
                    acc.DeviceId == account.DeviceId &&
                    acc.Email == account.Email &&
                    acc.Role == account.Role &&
                    acc.Password == "sW$grujBqGMdK.kXN0MXClE" &&
                    acc.PasswordExpirationDate == null 
                )
            ), Times.Once);
        }
    }
}