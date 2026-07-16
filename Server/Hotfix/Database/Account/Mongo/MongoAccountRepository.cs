using Fantasy;
using Fantasy.Database.Account.Mongo.Documents;
using MongoDB.Driver;

namespace Fantasy.Database.Account.Mongo;

/// <summary>
/// MongoDB persistence for account documents.
/// </summary>
public sealed class MongoAccountRepository
{
    private readonly IMongoCollection<AccountDocument> _accounts;
    private readonly IMongoCollection<LoginChallengeDocument> _loginChallenges;

    public MongoAccountRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _accounts = database.GetCollection<AccountDocument>(AccountDocument.CollectionName);
        _loginChallenges = database.GetCollection<LoginChallengeDocument>(LoginChallengeDocument.CollectionName);
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var accountIndexes = new[]
        {
            new CreateIndexModel<AccountDocument>(
                Builders<AccountDocument>.IndexKeys.Ascending(x => x.PhoneHash),
                new CreateIndexOptions { Name = "uq_accounts_phone_hash", Unique = true })
        };

        var challengeIndexes = new[]
        {
            new CreateIndexModel<LoginChallengeDocument>(
                Builders<LoginChallengeDocument>.IndexKeys.Ascending(x => x.ExpiresAt),
                new CreateIndexOptions { Name = "ttl_login_challenges_expiry", ExpireAfter = TimeSpan.Zero })
        };

        await Task.WhenAll(
            _accounts.Indexes.CreateManyAsync(accountIndexes, cancellationToken),
            _loginChallenges.Indexes.CreateManyAsync(challengeIndexes, cancellationToken));
    }

    public async Task<AccountDocument?> FindByPhoneHashAsync(string phoneHash, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneHash);
        return await _accounts.Find(x => x.PhoneHash == phoneHash).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AccountDocument> CreateAsync(string phoneHash, CancellationToken cancellationToken = default)
    {
        var account = AccountDocument.Create(phoneHash);
        await _accounts.InsertOneAsync(account, cancellationToken: cancellationToken);
        return account;
    }
}
