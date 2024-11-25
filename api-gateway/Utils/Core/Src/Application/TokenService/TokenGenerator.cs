namespace Application.Core
{
    public interface ITokenService<T>
    {
        public T GenerateToken(string payload, string role);
    }
}