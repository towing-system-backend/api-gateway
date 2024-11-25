namespace Application.Core
{
    public interface IEmailService<T>
    {
        public Task SendEmail(T emailInfo);
    }
}
