using Inotify;
using Inotify.Data;
using Inotify.Data.Models;
using Inotify.Sends;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;


namespace Inotify.Sends
{

    public class SendTaskManager
    {
        private static SendTaskManager m_Instance;

        public static SendTaskManager Instance
        {
            get
            {
                if (m_Instance == null) m_Instance = new SendTaskManager();
                return m_Instance;
            }
        }

        private readonly Thread m_analyseThread;

        private readonly List<Thread> m_sendThreads;

        private readonly BlockingCollection<SendMessage> m_sendMessages;

        private readonly BlockingCollection<SendMessage> m_analyseMessages;

        private readonly Dictionary<string, Type> m_sendMethodTemplateTypes;


        private SendTaskManager()
        {
            m_sendMessages = new BlockingCollection<SendMessage>();
            m_analyseMessages = new BlockingCollection<SendMessage>();
            m_sendMethodTemplateTypes = new Dictionary<string, Type>();
            m_sendThreads = new List<Thread>();

            var sendMethodTemplates = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(e => e.GetCustomAttribute<SendMethodKeyAttribute>() != null)
                .OrderBy(e => e.GetCustomAttribute<SendMethodKeyAttribute>().Order)
                .ToList();

            sendMethodTemplates.ForEach(sendMethodTemplate =>
            {
                var attribute = sendMethodTemplate.GetCustomAttribute<SendMethodKeyAttribute>();
                if (attribute != null)
                {
                    var key = attribute.Key;
                    m_sendMethodTemplateTypes.Add(key, sendMethodTemplate);
                }
            });

            m_analyseThread = null;
            m_analyseThread = new Thread(e => { SendLog(m_analyseThread); })
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            m_analyseThread.Start();
        }


        public EventHandler<SendMessage> OnMessageAdd;

        public EventHandler<SendMessage> OnSendSucessed;

        public EventHandler<SendMessage> OnSendFailed;

        public void Run()
        {
            var sendthread = SendCacheStore.GetSystemValue("sendthread");
            int.TryParse(sendthread, out int threadCount);
            threadCount = threadCount <= 0 ? 1 : threadCount;
            for (int i = 0; i < threadCount; i++)
            {
                Thread thread = null;
                thread = new Thread(e => { SendWork(thread); })
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                thread.Start();
                m_sendThreads.Add(thread);
            }
        }

        public void Stop()
        {
            foreach (var thread in m_sendThreads)
            {
                // m_messages.Add(new StopMessage());
                thread.Interrupt();
            }
            m_sendThreads.Clear();
        }

        public bool SendMessage(SendMessage message)
        {
            if (m_sendMessages.Count > 10000)
                return false;

            m_sendMessages.Add(message);

            OnMessageAdd?.Invoke(this, message);

            return true;
        }

        public Dictionary<string, InputTemeplate> GetInputTemeplates()
        {
            var sendTemplates = new Dictionary<string, InputTemeplate>();
            foreach (var sendMethodTemplateType in m_sendMethodTemplateTypes.Values)
            {
                var obj = Activator.CreateInstance(sendMethodTemplateType);
                var getTemeplateMethod = sendMethodTemplateType.GetMethod("GetTemeplate");
                if (getTemeplateMethod != null)
                {
                    if (getTemeplateMethod.Invoke(obj, null) is InputTemeplate temeplate && temeplate.Key != null)
                        sendTemplates.Add(temeplate.Key, temeplate);
                }
            }
            return sendTemplates;
        }

        public InputTemeplate? GetInputTemplate(string key)
        {
            var sendMethodTemplateType = m_sendMethodTemplateTypes[key];
            var obj = Activator.CreateInstance(sendMethodTemplateType);
            var getTemeplateMethod = sendMethodTemplateType.GetMethod("GetTemeplate");
            if (getTemeplateMethod != null)
            {
                var temeplate = getTemeplateMethod.Invoke(obj, null) as InputTemeplate;
                return temeplate;
            }
            return null;
        }

        private void SendWork(Thread thread)
        {
            while (true && thread.ThreadState != ThreadState.WaitSleepJoin)
            {
                try
                {
                    var message = m_sendMessages.Take();
                    var authData = DBManager.Instance.GetAuth(message.Token, out string temeplateId);
                    if (temeplateId != null && authData != null)
                    {
                        if (m_sendMethodTemplateTypes.ContainsKey(temeplateId))
                        {
                            var sendMethodTemplateActor = Activator.CreateInstance(m_sendMethodTemplateTypes[temeplateId]);
                            if (sendMethodTemplateActor != null)
                            {
                                var sendMethodType = sendMethodTemplateActor.GetType();
                                var compositonMethod = sendMethodType.GetMethod("Composition");
                                if (compositonMethod != null)
                                {
                                    compositonMethod.Invoke(sendMethodTemplateActor, new object[] { authData });
                                }

                                var sendMessageMethod = sendMethodType.GetMethod("SendMessage");
                                if (sendMessageMethod != null)
                                {
                                    var result = sendMessageMethod.Invoke(sendMethodTemplateActor, new object[] { message });
                                    if (result != null)
                                    {
                                        m_analyseMessages.Add(message);
                                        if ((bool)result)
                                        {
                                            OnSendSucessed?.Invoke(this, message);
                                            continue;
                                        }
                                    }

                                }
                            }
                        }
                    }

                    OnSendFailed?.Invoke(this, message);

                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
                finally
                {

                }
            }
        }

        private void SendLog(Thread thread)
        {
            while (true && thread.ThreadState != ThreadState.WaitSleepJoin)
            {
                var message = m_analyseMessages.Take();
                var date = DateTime.Now.ToString("yyyyMMdd");
                var authData = DBManager.Instance.GetAuth(message.Token, out string temeplateId);

                if (temeplateId != null)
                {
                    var sendInfo = DBManager.Instance.DBase.Query<SendInfo>().FirstOrDefault(e => e.Date == date && e.TemplateID == temeplateId);
                    if (sendInfo == null)
                    {
                        sendInfo = new SendInfo() { Date = date, TemplateID = temeplateId, Count = 1 };
                        DBManager.Instance.DBase.Insert(sendInfo);
                    }
                    else
                    {
                        sendInfo.Count++;
                        DBManager.Instance.DBase.Update(sendInfo, e => e.Count);
                    }
                }
            }
        }

    }
}
