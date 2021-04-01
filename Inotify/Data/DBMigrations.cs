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
            //V2版本允许多通道,激活标记放入SendAuthInfo表中，增加Active列
            //更新原有用户的激活通道
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
}
