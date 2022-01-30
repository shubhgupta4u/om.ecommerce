using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace om.account.model
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonElement(elementName: "_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        [BsonElement(elementName: "userName")]
        public string UserName { get; set; }
        [BsonElement(elementName: "mobile")]
        public string Mobile { get; set; }
        [BsonElement(elementName: "email")]
        public string Email { get; set; }
        [BsonElement(elementName: "isMobileVerfied")]
        public bool IsMobileVerfied { get; set; }
        [BsonElement(elementName: "isEmailVerfied")]
        public bool IsEmailVerfied { get; set; }
        [BsonElement(elementName: "createDate")]
        public DateTimeOffset CreateDate { get; set; }
        [BsonElement(elementName: "roles")]
        public IEnumerable<string> UserRoles { get; set; }
    }
}
