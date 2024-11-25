using Auth.Application;
using MongoDB.Driver;
using IOptional = Application.Core.Optional<Auth.Application.Account>;

namespace Auth.Infrastructure
{
    public class MongoAccountRepository : IAccountRepository
    {
        private readonly IMongoCollection<MongoAccount> _accountCollection;

        public MongoAccountRepository()
        {
            MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("CONNECTION_URI"));
            IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("DATABASE_NAME"));
            _accountCollection = database.GetCollection<MongoAccount>("accounts");
        }

        public async Task<IOptional> FindByEmail(string email)
        {
            var filter = Builders<MongoAccount>.Filter.Eq(account => account.Email, email);
            var res = await _accountCollection.Find(filter).FirstOrDefaultAsync();

            if (res == null) return IOptional.Empty();

            return IOptional.Of(
                new Account(
                    res.UserId,
                    res.Email,
                    res.Password,
                    res.PasswordExpirationDate
                )
            );
        }

        public async Task Save(Account account)
        {
            var filter = Builders<MongoAccount>.Filter.Eq(account => account.UserId, account.UserId);
            var update = Builders<MongoAccount>.Update
                .Set(account => account.Email, account.Email)
                .Set(account => account.Password, account.Password)
                .Set(account => account.PasswordExpirationDate, account.PasswordExpirationDate ?? null);

            await _accountCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }
    }
}