namespace Auth.Application
{
    public class Account(string userId, string supplierCompanyId, string? deviceId, string email, string role, string password, DateTime? passwordExpirationDate)
    { 
        public string UserId = userId;
        public string SupplierCompanyId = supplierCompanyId;
        public string? DeviceId = deviceId;
        public string Email = email;
        public string Role = role;
        public string Password = password;
        public DateTime? PasswordExpirationDate = passwordExpirationDate;
    }
}