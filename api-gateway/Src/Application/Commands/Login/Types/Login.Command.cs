namespace Auth.Application
{
    public record LoginCommand(
        string Email,
        string Password,
        string? DeviceId = null
    );
}