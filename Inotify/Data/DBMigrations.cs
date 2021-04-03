using Inotify.Common;
using Inotify.Data.Models;
using NPoco.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inotify.Data
{
    public class EmptyMigration : Migration, IMigration
    {
        protected override void execute()
        {

        }
    }

    public class LatestMigration : Migration, IMigration
    {
        protected override void execute()
        {
            if (!Migrator.TableExists<SystemInfo>())
            {
                Migrator.CreateTable<SystemInfo>(true).Execute();
                Migrator.Database.Insert(new SystemInfo()
                {
                    key = "administrators",
                    Value = "admin"
                });
            }

            if (!Migrator.TableExists<SendInfo>())
                Migrator.CreateTable<SendInfo>(true).Execute();

            if (!Migrator.TableExists<SendUserInfo>())
            {
                Migrator.CreateTable<SendUserInfo>(true).Execute();
                SendUserInfo userInfo = new SendUserInfo()
                {
                    Token = Guid.NewGuid().ToString("N").ToUpper(),
                    UserName = "admin",
                    Email = "admin@qq.com",
                    CreateTime = DateTime.Now,
                    Active = true,
                    Password = "123456".ToMd5()
                };
                Migrator.Database.Insert(userInfo);
            }

            if (!Migrator.TableExists<SendAuthInfo>())
            {
                Migrator.CreateTable<SendAuthInfo>(true).Execute();
            }
        }
    }

    public class V2UpdateMigration : Migration, IMigration
    {
        protected override void execute()
        {
            //V2版本允许多通道,激活标记放入SendAuthInfo表中，增加Active列，同时更新原有用户的激活通道
            Migrator.AlterTable<SendAuthInfo>().AddColumn(e => e.Active).Execute();
            Migrator.Database.UpdateMany<SendAuthInfo>().OnlyFields(e => e.Active).Execute(new SendAuthInfo() { Active = false });
            var activeUsers = Migrator.Database.Query<SendUserInfo>().ToList();
            activeUsers.ForEach(user =>
            {
                var sendUserInfo = Migrator.Database.Query<SendAuthInfo>().FirstOrDefault(e => e.Id == user.SendAuthId);
                if (sendUserInfo != null)
                {
                    sendUserInfo.Active = true;
                    Migrator.Database.Update(sendUserInfo, e => e.Active); ;
                }
            });
        }
    }

    public class V2001UpdateMigration : Migration, IMigration
    {
        protected override void execute()
        {
            //V2001版本增加SendInfo的key字段
            Migrator.AlterTable<SendAuthInfo>().AddColumn(e => e.Key).Execute();

            //对AuthInfo的AuthDate字段进行加密
            var sendAuthInfos = Migrator.Database.Query<SendAuthInfo>().ToList();
            sendAuthInfos.ForEach(sendAuthInfo =>
            {
                sendAuthInfo.AuthData = sendAuthInfo.AuthDataSave;
                Migrator.Database.Update(sendAuthInfo);
            });

            //添加bark密钥相关内容
            Migrator.Database.Insert(new SystemInfo()
            {
                key = "barkKeyId",
                Value = "TEg0VDlWNVU0Ug==".Base64Decode(),
            });
            Migrator.Database.Insert(new SystemInfo()
            {
                key = "barkTeamId",
                Value = "NVU4TEJSWEczQQ==".Base64Decode(),
            });
            Migrator.Database.Insert(new SystemInfo()
            {
                key = "barkPrivateKey",
                Value = "LS0tLS1CRUdJTiBQUklWQVRFIEtFWS0tLS0tCk1JR1RBZ0VBTUJNR0J5cUdTTTQ5QWdFR0NDcUdTTTQ5QXdFSEJIa3dkd0lCQVFRZzR2dEMzZzVMNUhnS0dKMitUMWVBMHRPaXZSRXZFQVkyZytqdVJYSmtZTDJnQ2dZSUtvWkl6ajBEQVFlaFJBTkNBQVNtT3MzSmtTeW9HRVdac1VHeEZzLzRwdzFySWxTVjJJQzE5TTh1M0c1a3EzNnVwT3d5RldqOUdpM0VqYzlkM3NDNytTSFJxWHJFQUpvdzgvN3RScFYrCi0tLS0tRU5EIFBSSVZBVEUgS0VZLS0tLS0=".Base64Decode()
            });
        }
    }
}
