namespace Inotify.Data.Models
{

    [NPoco.TableName("systemInfo")]
    [NPoco.PrimaryKey("key", AutoIncrement = false)]
    public class SystemInfo
    {
        [NPoco.Column("key")]
        public string key;

        [NPoco.Column("value")]
        public string Value;

        public SystemInfo()
        {

        }

        public SystemInfo(string key, string value)
        {
            this.key = key;
            Value = value;
        }
    }
}
