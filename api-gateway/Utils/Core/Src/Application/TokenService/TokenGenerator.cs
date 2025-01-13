namespace Application.Core
{
    public record TokenInfo(string Payload, string Role, string SupplierCompanyId);
    public interface ITokenService<T>
    {
        public T GenerateToken(TokenInfo tokenInfo);
    }
}