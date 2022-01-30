using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace om.account.model
{
    [BsonIgnoreExtraElements]
    public class UserCredential
    {
        [BsonId]
        [BsonElement(elementName: "_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement(elementName: "userId")]
        public string UserId { get; set; }
        [BsonElement(elementName: "password")]
        public string Password { get; set; }
        [BsonElement(elementName: "expiryDate")]
        public DateTimeOffset ExpiryDate { get; set; }
        [BsonElement(elementName: "isLocked")]
        public bool IsLocked { get; set; }
        [BsonElement(elementName: "isDeleted")]
        public bool IsDeleted { get; set; }
        [BsonElement(elementName: "isResetPwdOnFirstLogin")]
        public bool IsResetPwdOnFirstLogin { get; set; }
    }
}
