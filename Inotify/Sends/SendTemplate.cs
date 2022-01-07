using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Inotify.Sends
{
    public enum InputType
    {
        TEXT = 1,
        CHECK = 2,
        ENUM = 3,
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SendMethodKeyAttribute : Attribute
    {
        public string Key;

        public bool Open;

        public string Name;

        public int Order;

        public string Waring;

        public SendMethodKeyAttribute(string key, string name, bool open = true, string waring = "")
        {
            Key = key;
            Name = name;
            Open = open;
            Waring = waring;
        }
    }

    public class InputTypeValue
    {

        public InputTypeValue()
        {
            Show = true;
            Readonly = false;
        }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Default { get; set; }
        public InputType? Type { get; set; }
        public int? Order { get; set; }
        public string? Value { get; set; }

        public string? Warning { get; set; }

        public bool Show { get; set; }

        public bool Readonly { get; set; }
    }

    public class InputTemeplate
    {
        public string? Type { get; set; }
        public string? TypeName { get; set; }
        public string? Key { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public string Warning { get; set; }
        public string? AuthData { get; set; }
        public int? SendAuthId { get; set; }
        public List<InputTypeValue>? Values { get; set; }

        public string TemplateToAuth()
        {
            var jObject = new JObject();

            if (Values != null)
            {
                foreach (var item in Values)
                {
                    if (item.Name != null && item.Value != null)
                    {
                        if (item.Type == InputType.CHECK)
                        {
                            jObject.Add(item.Name, item.Value.ToLower() == "true");
                        }
                        else
                        {
                            jObject.Add(item.Name, item.Value);
                        }
                    }
                }
            }

            return jObject.ToString();
        }

        public void AuthToTemplate(string authInfo)
        {
            if (!string.IsNullOrEmpty(authInfo))
            {
                var jObject = JObject.Parse(authInfo);

                if (Values != null)
                {
                    foreach (var value in Values)
                    {
                        if (value.Name != null)
                        {
                            if (jObject.TryGetValue(value.Name, out JToken? jtoken))
                            {
                                value.Value = jtoken == null ? "" : jtoken.Value<string>();
                            }
                        }
                    }
                }
            }
            AuthData = authInfo;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    [JsonObject(MemberSerialization.OptIn)]
    public class InputTypeAttribte : Attribute
    {
        public InputTypeValue InputTypeData { get; set; }


        private InputTypeAttribte(int order, string name, string description, string defaultValue, InputType type, bool show = true, bool readOnly = false)
        {
            InputTypeData = new InputTypeValue
            {
                Name = name,
                Description = description,
                Default = defaultValue,
                Order = order,
                Type = type,
                Show = show,
                Readonly = readOnly
            };
        }

        public InputTypeAttribte(int order, string name, string description, string defaultValue, bool show = true, bool readOnly = false)
            : this(order, name, description, defaultValue, InputType.TEXT, show, readOnly)
        {

        }

        public InputTypeAttribte(int order, string name, string description, bool defaultValue, bool show = true, bool readOnly = false)
           : this(order, name, description, "", InputType.CHECK, show, readOnly)
        {
            InputTypeData.Default = defaultValue ? "是" : "否";
        }
    }


    public abstract class SendTemplate<T>
    {
        public T Auth { get; set; }

        public void Composition(string authInfo)
        {
            Auth = JsonConvert.DeserializeObject<T>(authInfo);
        }

        public InputTemeplate GetTemeplate()
        {
            var values = typeof(T)
                .GetProperties()
                .SelectMany(e => e.GetCustomAttributes(typeof(InputTypeAttribte), false))
                .Cast<InputTypeAttribte>()
                .Select(e => e.InputTypeData)
                .ToList();

            var sendMethodKeyAttribute = GetType().GetCustomAttribute<SendMethodKeyAttribute>();

            if (sendMethodKeyAttribute != null)
            {
                return new InputTemeplate()
                {
                    Key = "",
                    Name = sendMethodKeyAttribute.Name,
                    Type = sendMethodKeyAttribute.Key,
                    TypeName = sendMethodKeyAttribute.Name,
                    Warning = sendMethodKeyAttribute.Waring,
                    Values = values
                };
            }

            return null;
        }

        public abstract bool SendMessage(SendMessage message);


        protected WebProxy GetProxy()
        {
            if (SendCacheStore.GetSystemValue("proxyenable") == "true")
            {
                var proxyurl = SendCacheStore.GetSystemValue("proxy");
                if (proxyurl != null)
                {

                    WebProxy proxy = new WebProxy
                    {
                        Address = new Uri(proxyurl)
                    };
                    return proxy;
                }
            }
            return null;
        }

        protected bool IsUrl(string url)
        {
            var regex = new Regex(@"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
            if (!regex.IsMatch(url))
            {
                return false;
            }
            return true;
        }

        protected bool IsImage(string url)
        {
            var regex = new Regex(@".*?(jpeg|jpe|png|jpg)");
            if (!regex.IsMatch(url))
            {
                return false;
            }
            return true;
        }

        protected byte[] GetImage(string url)
        {
            if (IsUrl(url) && IsImage(url))
            {
                using (WebClient mywebclient = new WebClient())
                {
                    return mywebclient.DownloadData(url);
                }
      
            }
            return null;
        }
    }


}
