using MongoDB.Bson.Serialization.Attributes;

namespace Fantasy.Database.Account.Mongo.Documents;

/// <summary>
/// Short-lived SMS verification challenge. The actual code is never persisted.
/// </summary>
public sealed class LoginChallengeDocument
{
    public const string CollectionName = "login_challenges";

    [BsonRequired]
    public string PhoneHash = string.Empty;

    [BsonRequired]
    public string CodeHash = string.Empty;

    public DateTime ExpiresAt;
}
