namespace RabbitMQ.Contracts
{
    public interface IRabbitMQMessage { };

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
    ): IRabbitMQMessage;

    public record CreateTowDriver(
        string Id,
        string SupplierCompanyId,
        string Name,
        string Email,
        string LicenseOwnerName,
        DateOnly LicenseIssueDate,
        DateOnly LicenseExpirationDate,
        string MedicalCertificateOwnerName,
        int MedicalCertificateAge,
        DateOnly MedicalCertificateIssueDate,
        DateOnly MedicalCertificateExpirationDate,
        int IdentificationNumber
    ): IRabbitMQMessage;
}
