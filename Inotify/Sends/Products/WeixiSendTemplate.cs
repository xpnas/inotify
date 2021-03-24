using Inotify.Common;
using Inotify.Sends;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Inotify.Sends.Products
{

    public class WeixiAuth
    {
        [InputTypeAttribte(0, "Corpid", "企业ID", "Corpid")]
        public string Corpid { get; set; }

        [InputTypeAttribte(1, "Corpsecret", "密钥", "Corpsecret")]
        public string Corpsecret { get; set; }

        [InputTypeAttribte(2, "AgentID", "应用ID", "AgentID")]
        public string AgentID { get; set; }

        [InputTypeAttribte(2, "OpengId", "OpengId", "@all")]
        public string OpengId { get; set; }
    }


    [SendMethodKey("409A30D5-ABE8-4A28-BADD-D04B9908D763", "企业微信", Order = 0)]
    public class WeixiSendTemplate : SendTemplate<WeixiAuth>
    {
        public override WeixiAuth Auth { get; set; }

        public override bool SendMessage(SendMessage message)
        {
            if (Auth == null) return false;

            var token = GetAccessToken();
            if (token == null) return false;

            return PostMail(token, message.Title, message.Data);
        }

        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        private string GetAccessToken()
        {
            var key = Auth.Corpid + Auth.AgentID + Auth.Corpsecret;
            var toekn = SendCacheStore.Get(key);
            if (toekn == null)
            {
                var url = string.Format(@"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}", Auth.Corpid, Auth.Corpsecret);

                WebRequest request = WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                using WebResponse response = request.GetResponse();
                using Stream streamResponse = response.GetResponseStream();
                StreamReader reader = new StreamReader(streamResponse);
                string responseFromServer = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    if (JsonConvert.DeserializeObject(responseFromServer) is JObject res)
                    {
                        if (res.TryGetValue("access_token", out JToken? jtoken))
                        {
                            toekn = jtoken.ToString();
                        }
                    }
                }
                reader.Close();
            }

            if (toekn != null)
                SendCacheStore.Set(key, toekn, DateTimeOffset.Now.AddHours(2));

            return toekn;
        }

        private bool PostMail(string accessToken, string title, string? data)
        {
            var uri = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=" + accessToken;

            var content = string.IsNullOrEmpty(data) ? title : title + "\n" + data;
            var isImage = !String.IsNullOrEmpty(title) && IsUrl(title) && IsImage(title) && String.IsNullOrEmpty(data);
            var imageData = isImage ? GetImage(title) : null;
            string mediaId = null;
            if (imageData != null)
                mediaId = UpLoadIMage(accessToken, imageData);

            //创建请求
            WebRequest myWebRequest = WebRequest.Create(uri);
            myWebRequest.Credentials = CredentialCache.DefaultCredentials;
            myWebRequest.ContentType = "application/json;charset=UTF-8";
            myWebRequest.Method = "POST";

            //向服务器发送的内容
            using (Stream streamResponse = myWebRequest.GetRequestStream())
            {
                object jsonObject;
                if (isImage && imageData != null && mediaId != null)
                {
                    jsonObject = new
                    {
                        touser = Auth.OpengId,
                        agentid = Auth.AgentID,
                        msgtype = "image",
                        image = new
                        {
                            media_id = mediaId
                        },
                        safe = 0
                    };
                }
                else
                {
                    jsonObject = new
                    {
                        touser = Auth.OpengId,
                        msgtype = "text",
                        agentid = Auth.AgentID,
                        text = new
                        {
                            content,
                        },
                        safe = 0
                    };
                }

                string paramString = JsonConvert.SerializeObject(jsonObject, Formatting.None);
                byte[] byteArray = Encoding.UTF8.GetBytes(paramString);
                //向请求中写入内容
                streamResponse.Write(byteArray, 0, byteArray.Length);
            }
            //创建应答
            WebResponse myWebResponse = myWebRequest.GetResponse();
            //创建应答的读写流
            string responseFromServer;
            using (Stream streamResponse = myWebResponse.GetResponseStream())
            {
                StreamReader streamRead = new StreamReader(streamResponse);
                responseFromServer = streamRead.ReadToEnd();
            }
            //关闭应答
            myWebResponse.Close();

            return true;
        }

        private string UpLoadIMage(string accessToken, byte[] bytes)
        {
            try
            {
                var uri = string.Format("https://qyapi.weixin.qq.com/cgi-bin/media/upload?access_token={0}&type=file", accessToken);
                string boundary = DateTime.Now.Ticks.ToString("X");
                WebRequest request = WebRequest.Create(uri);
                request.Method = "POST";
                request.Credentials = CredentialCache.DefaultCredentials;
                request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
                var itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                var endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                var sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", boundary));
                var postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());

                Stream postStream = request.GetRequestStream();
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                postStream.Write(bytes, 0, bytes.Length);
                postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                postStream.Close();

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream instream = response.GetResponseStream();
                StreamReader sr = new StreamReader(instream, Encoding.UTF8);
                string content = sr.ReadToEnd();
                var result = JObject.Parse(content);
                return result.Value<string>("media_id").ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
