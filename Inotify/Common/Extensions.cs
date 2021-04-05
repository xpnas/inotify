using System;
using System.Security.Cryptography;
using System.Text;

namespace Inotify.Common
{
    public static class Extensions
    {
        private static int rep = 0;

        public static string ToMd5(this string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            string result = BitConverter.ToString(md5.ComputeHash(bytes));
            return result.Replace("-", "");
        }

        public static string UrlEncode(this string str)
        {
            string urlStr = System.Web.HttpUtility.UrlEncode(str);
            return urlStr;
        }

        public static string Base64Encode(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }

            byte[] bytes = (Encoding.UTF8.GetBytes(source));
            return Convert.ToBase64String(bytes);

        }

        public static string Base64Decode(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }

            var bytes = Convert.FromBase64String(source);
            return System.Text.Encoding.Default.GetString(bytes);
        }

        public static string GenerateCheckCode(this int codeCount)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + rep;
            rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str += ch.ToString();
            }
            return str;
        }

        public static long ToUTC(this DateTime time)
        {
            TimeSpan ts = time - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static long ToUnix(this DateTime time)
        {
            var expiration = time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (long)expiration.TotalSeconds;
        }
    }
}
