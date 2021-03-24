using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inotify.Data.Models
{
    [NPoco.TableName("sendInfo")]
    [NPoco.PrimaryKey(new string[] { "templateID", "date" }, AutoIncrement = false)]
    public class SendInfo
    {
        [NPoco.Column("templateID")]
        public string TemplateID { get; set; }

        [NPoco.Column("date")]
        public string Date { get; set; }

        [NPoco.Column("count")]
        public int Count { get; set; }
    }
}
