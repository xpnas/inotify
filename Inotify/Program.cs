using Inotify.Data;
using Inotify.Sends;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Inotify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DBManager.Instance.Run();
            SendTaskManager.Instance.Run();

            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            };
            try
            {
                var appManager = StartUpManager.Load();
                do
                {
                    appManager.Start(args);
                } while (appManager.Restarting);
            }
            catch (Exception)
            {

            }
        }
    }
}
