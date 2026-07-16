using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fantasy;

/// <summary>
/// MongoDB document stored in the accounts collection.
/// </summary>
public sealed class AccountDocument
{
    public const string CollectionName = "accounts";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string AccountId = ObjectId.GenerateNewId().ToString();

    // HMAC-SHA256 of the normalized E.164 phone number. Do not store a raw phone number here.
    [BsonRequired]
    public string PhoneHash = string.Empty;

    // Null means this account can only use SMS verification to sign in.
    [BsonIgnoreIfNull]
    public string? PasswordHash;

    public static AccountDocument Create(string phoneHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneHash);

        return new AccountDocument
        {
            PhoneHash = phoneHash,
        };
    }
}
