﻿namespace Inotify.Data.Models
{
    [NPoco.TableName("userInfo")]
    [NPoco.PrimaryKey("id")]
    public class SendUserInfo : SystemUserInfo
    {
        [NPoco.Column("token")]
        public string? Token { get; set; }

        [NPoco.Column("sendAuthId")]
        public int SendAuthId { get; set; }
    }
}
