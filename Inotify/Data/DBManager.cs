using Inotify.Common;
using Inotify.Data.Models;
using Inotify.Data.Models.System;
using Inotify.Sends.Products;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel;
using Newtonsoft.Json;
using NPoco;
using NPoco.Migrations;
using System;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Inotify.Data
{
    public class DBManager
    {
        private readonly SqliteConnection m_dbConnection;

        private readonly Migrator m_migrator;

        private readonly string m_dataPath = "inotify_data";

        private readonly string m_jwtPath;

        private readonly string m_dbPath;

        private JwtInfo m_JWT;

        public JwtInfo JWT
        {
            get { return m_JWT; }
            set
            {
                m_JWT = value;
                File.WriteAllText(m_jwtPath, JsonConvert.SerializeObject(JWT));
            }
        }

        public Database DBase { get; private set; }

        private static DBManager? m_Instance;

        public static DBManager Instance
        {
            get
            {
                if (m_Instance == null) m_Instance = new DBManager();
                return m_Instance;
            }
        }

        private DBManager()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                m_jwtPath = Path.Combine(Directory.GetCurrentDirectory(), "/" + m_dataPath + "/jwt.json");
                m_dbPath = Path.Combine(Directory.GetCurrentDirectory(), "/" + m_dataPath + "/data.db");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var dataPath = Path.Combine(Directory.GetCurrentDirectory(), m_dataPath);
                if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);

                m_jwtPath = Path.Combine(Directory.GetCurrentDirectory(), m_dataPath + "/jwt.json");
                m_dbPath = Path.Combine(Directory.GetCurrentDirectory(), m_dataPath + "/data.db");
            }

            if (!File.Exists(m_jwtPath))
            {
                var bytes = new byte[128];
                var ranndom = new Random();
                ranndom.NextBytes(bytes);
                var key = Convert.ToBase64String(bytes);

                JWT = new JwtInfo()
                {
                    ClockSkew = 10,
                    Audience = "Inotify",
                    Issuer = "Inotify",
                    IssuerSigningKey = key,
                    Expiration = 36000,
                };

                File.WriteAllText(m_jwtPath, JsonConvert.SerializeObject(JWT));
            }
            else
            {
                JWT = JsonConvert.DeserializeObject<JwtInfo>(File.ReadAllText(m_jwtPath));
            }


            m_dbConnection = new SqliteConnection(string.Format("Data Source={0}", m_dbPath));
            if (m_dbConnection.State == ConnectionState.Closed)
                m_dbConnection.Open();

            DBase = new NPoco.Database(m_dbConnection, DatabaseType.SQLite);
            m_migrator = new Migrator(DBase);
        }

        public bool IsSendKey(string token)
        {
            return DBase.Query<SendUserInfo>().Any(e => e.Token == token);
        }

        public bool IsUser(string userName)
        {
            return DBase.Query<SendUserInfo>().Any(e => e.UserName == userName);
        }

        public SendUserInfo GetUser(string userName)
        {
            return DBase.Query<SendUserInfo>().First(e => e.UserName == userName);
        }

        public string GetAuth(string token, out string guid)
        {
            guid = string.Empty;
            var upToekn = token.ToUpper();
            var userInfo = DBManager.Instance.DBase.Query<SendUserInfo>().FirstOrDefault(e => e.Token == upToekn && e.Active);
            if (userInfo == null) return null;

            var authInfo = DBManager.Instance.DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.Id == userInfo.SendAuthId && e.UserId == userInfo.Id);
            if (authInfo == null)
                return null;

            guid = authInfo.SendMethodTemplate;
            return authInfo.AuthData;
        }


        public void Run()
        {
            if (!m_migrator.TableExists<SendInfo>())
                m_migrator.CreateTable<SendInfo>(true).Execute();

            if (!m_migrator.TableExists<SystemInfo>())
            {
                m_migrator.CreateTable<SystemInfo>(true).Execute();

                DBase.Insert(new SystemInfo()
                {
                    key = "administrators",
                    Value = "admin"
                });
            }

            if (!m_migrator.TableExists<SendUserInfo>())
            {
                m_migrator.CreateTable<SendUserInfo>(true).Execute();
                SendUserInfo userInfo = new SendUserInfo()
                {
                    Token = "112D77BAD9704FFEAECD716B5678DFBE".ToUpper(),
                    UserName = "admin",
                    Email = "admin@qq.com",
                    CreateTime = DateTime.Now,
                    Active = true,
                    Password = "123456".ToMd5()
                };
                DBase.Insert(userInfo);
            }

            if (!m_migrator.TableExists<SendAuthInfo>())
            {
                m_migrator.CreateTable<SendAuthInfo>(true).Execute();
            }
        }
    }
}
