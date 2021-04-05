using Inotify.Common;
using System;
namespace Inotify.Data.Models
{
    [NPoco.TableName("sendAuthInfo")]
    [NPoco.PrimaryKey("id")]
    public class SendAuthInfo
    {
        [NPoco.Column("id")]
        public int Id { get; set; }

        [NPoco.Column("userId")]
        public int UserId { get; set; }

        [NPoco.Column("name")]
        public string Name { get; set; }

        [NPoco.Column("sendMethodTemplate")]
        public string SendMethodTemplate { get; set; }

        [NPoco.Column("authData")]
        public string AuthDataSave { get; set; }

        [NPoco.Ignore]
        public string AuthData
        {
            get => AuthDataSave.Base64Decode();
            set => AuthDataSave = value.Base64Encode();
        }

        [NPoco.Column("modifyTime")]
        public DateTime ModifyTime { get; set; }

        [NPoco.Column("createTime")]
        public DateTime CreateTime { get; set; }

        [NPoco.Column("active")]
        public bool Active { get; set; }

        [NPoco.Column("key")]
        public string Key { get; set; }

    }
}
