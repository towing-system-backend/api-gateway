using Application.Core;
using Moq;
using Auth.Application;
using RabbitMQ.Contracts;
using Xunit;

namespace Auth.Tests
{
    public class SignUpCommandHandlerTests
    {
        private readonly Mock<IdService<string>> _idServiceMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<ICryptoService> _cryptoServiceMock;
        private readonly Mock<IEmailService<EmailInfo>> _emailServiceMock;
        private readonly Mock<IMessageBrokerService> _messageBrokerServiceMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly SignUpCommandHandler _signUpCommandHandler;

        public SignUpCommandHandlerTests()
        {
            _idServiceMock = new Mock<IdService<string>>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _cryptoServiceMock = new Mock<ICryptoService>();
            _emailServiceMock = new Mock<IEmailService<EmailInfo>>();
            _messageBrokerServiceMock = new Mock<IMessageBrokerService>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _signUpCommandHandler = new SignUpCommandHandler(
                _idServiceMock.Object,
                _passwordServiceMock.Object,
                _cryptoServiceMock.Object,
                _emailServiceMock.Object,
                _messageBrokerServiceMock.Object,
                _accountRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Should_Not_SignUp_When_User_Already_Exists()
        {
            // Arrange
            var command = new SignUpCommand(
                "c9db506f-4f8b-4f55-a75a-2c35c3bb6d16",
                "Andres Hernandez",
                "https://t3.ftcdn.net/jpg/02/43/12/34/360_F_243123463_zTooub557xEWABDLk0jJklDyLSGl2jrr.jpg",
                "andreshg@gmail.com",
                "TowDriver",
                "Active",
                "04145874152",
                25547415
            );

            var existingAccount = new Account(
                "07e24b54-4d06-479b-8249-9e98dffa7b28",
                "00789dad-4d0c-493f-b84c-5560caea24a1",
                "4779c4ec-0d54-4731-8885-69122dd6db1c",
                "andreshg@gmail.com",
                "TowDriver",
                "$2a$111$oDRvMB6ogg1$HRdae1nzRT57g0v/bM",
                DateTime.UtcNow.AddYears(1)
            );

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Of(existingAccount));

            // Act
            var result = await _signUpCommandHandler.Execute(command);

            // Assert
            Assert.True(result.IsError);
            var exception = Assert.Throws<UserAlreadyExistsError>(() => result.Unwrap());
            Assert.Equal("User already exists.", exception.Message);

            _accountRepositoryMock.Verify(repo => repo.Save(It.IsAny<Account>()), Times.Never);
            _emailServiceMock.Verify(service => service.SendEmail(It.IsAny<EmailInfo>()), Times.Never);
            _messageBrokerServiceMock.Verify(service => service.Publish(It.IsAny<CreateUser>()), Times.Never);
        }

        [Fact]
        public async Task Should_SignUp_User_Successfully()
        {
            // Arrange
            var command = new SignUpCommand(
                "c9db506f-4f8b-4f55-a75a-2c35c3bb6d16",
                "Andres Hernandez",
                "https://t3.ftcdn.net/jpg/02/43/12/34/360_F_243123463_zTooub557xEWABDLk0jJklDyLSGl2jrr.jpg",
                "andreshg@gmail.com",
                "TowDriver",
                "Active",
                "04145874152",
                25547415
            );

            _accountRepositoryMock.Setup(repo => repo.FindByEmail(command.Email))
                .ReturnsAsync(Optional<Account>.Empty());

            _idServiceMock.Setup(service => service.GenerateId())
                .Returns("011656b1-fcfa-4f64-a93c-5cc74c8f60df");

            _passwordServiceMock.Setup(service => service.GeneratePassword())
                .Returns("1YDGXAD");

            _cryptoServiceMock.Setup(service => service.Hash("1YDGXAD"))
                .Returns("sW$grujBqGMdK.kXN0MXClE");

            _emailServiceMock.Setup(service => service.SendEmail(It.IsAny<EmailInfo>()))
                .Returns(Task.CompletedTask);

            _messageBrokerServiceMock.Setup(service => service.Publish(It.IsAny<CreateUser>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _signUpCommandHandler.Execute(command);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal("011656b1-fcfa-4f64-a93c-5cc74c8f60df", result.Unwrap().UserId);

            _accountRepositoryMock.Verify(repo => repo.Save(It.Is<Account>(account =>
                    account.UserId == "011656b1-fcfa-4f64-a93c-5cc74c8f60df" &&
                    account.Email == command.Email &&
                    account.Role == command.Role &&
                    account.Password == "sW$grujBqGMdK.kXN0MXClE" &&
                    account.PasswordExpirationDate != null
                )
            ), Times.Once);

            _emailServiceMock.Verify(service => service.SendEmail(It.Is<EmailInfo>(email =>
                    email.To == command.Email &&
                    email.Subject == "Welcome Email" &&
                    email.Body.Contains("Your password is: 1YDGXAD")
                )
            ), Times.Once);

            _messageBrokerServiceMock.Verify(service => service.Publish(It.Is<CreateUser>(account =>
                    account.Id == "011656b1-fcfa-4f64-a93c-5cc74c8f60df" &&
                    account.SupplierCompanyId == command.SupplierCompanyId &&
                    account.Name == command.Name &&
                    account.Image == command.Image &&
                    account.Email == command.Email &&
                    account.Role == command.Role &&
                    account.Status == command.Status &&
                    account.PhoneNumber == command.PhoneNumber &&
                    account.IdentificationNumber == command.IdentificationNumber
                )
            ), Times.Once);
        }
    }
}
