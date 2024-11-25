namespace Auth.Infrastructure
{
    public record SignUpDto(
        string UserName,
        string IdentificationNumber,
        string Email
    );
}

