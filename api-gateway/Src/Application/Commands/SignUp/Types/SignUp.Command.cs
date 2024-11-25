namespace Auth.Application
{
    public record SignUpCommand(string UserName, string IdentificationNumber, string Email);
}