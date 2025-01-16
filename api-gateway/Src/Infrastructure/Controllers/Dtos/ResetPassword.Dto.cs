namespace Auth.Infrastructure
{
    public record ResetPasswordDto(
        string Email,
        string NewPassword,
        string NewPasswordConfirmation
    );
}