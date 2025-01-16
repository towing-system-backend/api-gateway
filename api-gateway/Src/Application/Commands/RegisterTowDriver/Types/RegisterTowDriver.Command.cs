namespace Auth.Application
{
    public record RegisterTowDriverCommand(
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
    );
}