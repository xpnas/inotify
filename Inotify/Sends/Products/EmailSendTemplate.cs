using FluentEmail.Core;
using FluentEmail.Liquid;
using FluentEmail.Smtp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Inotify.Sends.Products
{

    public class EmailAuth
    {
        [InputTypeAttribte(0, "FromName", "发件人", "管理员")]
        public string FromName { get; set; }

        [InputTypeAttribte(0, "From", "发件地址", "abc@qq.com")]
        public string From { get; set; }

        [InputTypeAttribte(1, "Password", "发件密码", "123456")]
        public string Password { get; set; }

        [InputTypeAttribte(2, "Host", "SMTP地址", "smtp.qq.com")]
        public string Host { get; set; }

        [InputTypeAttribte(2, "Port", "SMTP端口", "587")]
        public int Port { get; set; }

        [InputTypeAttribte(3, "EnableSSL", "SSL", "true|false")]
        public bool EnableSSL { get; set; }

        [InputTypeAttribte(4, "To", "收件箱", "abcd@qq.com")]
        public string To { get; set; }
    }


    [SendMethodKey("EA2B43F7-956C-4C01-B583-0C943ABB36C3", "邮件推送", Order = 1)]
    public class EmailSendTemplate : SendTemplate<EmailAuth>
    {
        public override EmailAuth Auth { get; set; }

        public override bool SendMessage(SendMessage message)
        {
            var smtpSender = new SmtpSender(new SmtpClient()
            {
                EnableSsl = Auth.EnableSSL,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = Auth.Port,
                Host = Auth.Host,
                Credentials = new NetworkCredential(Auth.From, Auth.Password),
            });

            var email =
                Email.From(Auth.From, Auth.FromName)
              .Subject(message.Title)
              .Body(message.Data ?? "")
              .To(Auth.To);

            smtpSender.SendAsync(email);

            return true;

        }
    }
}
