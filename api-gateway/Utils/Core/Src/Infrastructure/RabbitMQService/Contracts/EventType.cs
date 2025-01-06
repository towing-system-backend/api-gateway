namespace RabbitMQ.Contracts
{
    public record EventType(
        object Context
    );

    public record CreateUser(
        string Id,
        string SupplierCompanyId,
        string Name,
        string Image,
        string Email,
        string Role,
        string Status,
        string PhoneNumber,
        int IdentificationNumber
    );
}
