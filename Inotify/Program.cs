using Inotify.Data;
using Inotify.Sends;
using Newtonsoft.Json;
using System;

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
