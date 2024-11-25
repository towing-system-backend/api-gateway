namespace Application.Core
{
    public interface ICryptoService
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}