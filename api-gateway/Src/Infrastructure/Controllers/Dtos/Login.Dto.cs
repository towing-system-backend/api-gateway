namespace Auth.Infrastructure
{
    public record LoginDto(
        string Email,
        string Password
    );
}
