using Inotify.Data.Models;
using Inotify.Data.Models.System;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using NPoco;
using NPoco.Migrations;
using NPoco.Migrations.CurrentVersion;
using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace Inotify.Data
{
    public class DBManager
    {
        private readonly string MigrationName = "inotify";

        private readonly SqliteConnection m_dbConnection;

        private readonly Migrator m_migrator;

        private readonly string m_dataPath = "inotify_data";

        private readonly string m_jwtPath;

        private readonly string m_dbPath;

        private JwtInfo m_JWT;

        public JwtInfo JWT
        {
            get => m_JWT;
            set
            {
                m_JWT = value;
                File.WriteAllText(m_jwtPath, JsonConvert.SerializeObject(JWT));
            }
        }

        public readonly string Inotify_Data;

        public Database DBase { get; private set; }

        private static DBManager? m_Instance;

        public static DBManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new DBManager();
                }

                return m_Instance;
            }
        }

        private DBManager()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Inotify_Data = Path.Combine(Directory.GetCurrentDirectory(), m_dataPath);
                if (!Directory.Exists(Inotify_Data))
                {
                    Directory.CreateDirectory(Inotify_Data);
                }

                m_jwtPath = Path.Combine(Directory.GetCurrentDirectory(), "/" + m_dataPath + "/jwt.json");
                m_dbPath = Path.Combine(Directory.GetCurrentDirectory(), "/" + m_dataPath + "/data.db");


            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Inotify_Data = Path.Combine(Directory.GetCurrentDirectory(), m_dataPath);
                if (!Directory.Exists(Inotify_Data))
                {
                    Directory.CreateDirectory(Inotify_Data);
                }

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
            {
                m_dbConnection.Open();
            }

            DBase = new Database(m_dbConnection, DatabaseType.SQLite)
            {
                KeepConnectionAlive = true
            };
            m_migrator = new Migrator(DBase);
        }

        public bool IsToken(string token, out bool hasActive)
        {
            hasActive = false;
            var userInfo = DBase.Query<SendUserInfo>().FirstOrDefault(e => e.Token == token);
            if (userInfo != null)
            {
                hasActive = DBase.Query<SendAuthInfo>().Any(e => e.UserId == userInfo.Id && e.Active);
                return true;
            }

            return false;
        }

        public bool IsSendKey(string key, out bool isActive, out string token)
        {
            isActive = false;
            token = null;
            var sendAuthInfo = DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.Key == key);
            if (sendAuthInfo != null)
            {
                var userInfo = DBase.Query<SendUserInfo>().FirstOrDefault(e => e.Id == sendAuthInfo.UserId);
                if (userInfo != null&& userInfo.Token!=null)
                {
                    token = userInfo.Token;
                }
                isActive = sendAuthInfo.Active;
                return true;
            }
            return false;
        }

        public bool IsUser(string userName)
        {
            return DBase.Query<SendUserInfo>().Any(e => e.UserName == userName);
        }

        public SendUserInfo GetUser(string userName)
        {
            return DBase.Query<SendUserInfo>().FirstOrDefault(e => e.UserName == userName);
        }

        public string GetSendAuthInfo(string key, out string guid)
        {
            guid = null;
            var authInfo = DBManager.Instance.DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.Key== key.ToUpper());
            if (authInfo == null)
            {
                return null;
            }

            guid = authInfo.SendMethodTemplate;
            return authInfo.AuthData;
        }

        public void GetSendAuthInfos(string token, string key, out SendAuthInfo[] sendAuthInfos)
        {
            sendAuthInfos = null;
            var upToekn = token.ToUpper();
            var userInfo = DBManager.Instance.DBase.Query<SendUserInfo>().FirstOrDefault(e => e.Token == upToekn && e.Active);
            if (userInfo != null)
            {
                if (string.IsNullOrEmpty(key))
                {
                    sendAuthInfos = DBManager.Instance.DBase.Query<SendAuthInfo>().Where(e => e.UserId == userInfo.Id && e.Active).ToArray();
                }
                else
                {
                    sendAuthInfos = DBManager.Instance.DBase.Query<SendAuthInfo>().Where(e => e.UserId == userInfo.Id && e.Active && e.Key == key).ToArray();
                }
            }

        }

        public void Run()
        {

            var codeVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var versionProvider = new DatabaseCurrentVersionProvider(DBase);

            if (!m_migrator.TableExists<SystemInfo>())
            {
                var migrationBuilder = new MigrationBuilder(MigrationName, DBase);
                migrationBuilder.Append(new Version(codeVersion.ToString()), new LatestMigration());
                migrationBuilder.Execute();
                versionProvider.SetMigrationVersion(MigrationName, new Version(codeVersion.ToString()));
            }
            else
            {
                if (versionProvider.GetMigrationVersion(MigrationName).ToString() == "0.0")
                {
                    versionProvider.SetMigrationVersion(MigrationName, new Version(1, 0, 0, 0));
                }

                var builder = new MigrationBuilder(MigrationName, DBase);
                builder.Append(new Version(2, 0, 0, 0), new V2UpdateMigration());
                builder.Append(new Version(2, 0, 0, 1), new V2001UpdateMigration());
                builder.Append(new Version(2, 0, 0, 4), new V2004UpdateMigration());
                builder.Execute();
            }
        }
    }
}
