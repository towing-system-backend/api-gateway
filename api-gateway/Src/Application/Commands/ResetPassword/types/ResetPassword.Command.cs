namespace Auth.Application
{
    public record ResetPasswordCommand(
        string Email,
        string NewPassword,
        string NewPasswordConfirmation
    );
}