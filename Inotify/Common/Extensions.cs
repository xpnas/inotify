using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Inotify.Common
{
    public static class Extensions
    {
        /// <summary>
        /// MD5加密字符串（32位大写）
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>加密后的字符串</returns>
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
            byte[] bytes = (Encoding.UTF8.GetBytes(source));
            return Convert.ToBase64String(bytes);

        }

        public static string Base64Decode(this string source)
        {
            var bytes = Convert.FromBase64String(source);
            return System.Text.Encoding.Default.GetString(bytes);
        }
    }
}
