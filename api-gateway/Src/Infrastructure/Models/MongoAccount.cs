using MongoDB.Bson.Serialization.Attributes;

namespace Auth.Infrastructure;

public class MongoAccount(string userId, string email, string password, DateTime? passwordExpirationDate)
{
    [BsonId]
    public string UserId = userId;
    public string Email = email;
    public string Password = password;
    public DateTime? PasswordExpirationDate = passwordExpirationDate;
}
