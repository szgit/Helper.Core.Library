/*
 * 作用：利用 Smtp 发送邮件。
 * */
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace Helper.Core.Library
{
    #region 邮件帮助辅助类
    /// <summary>
    /// 邮件实体对象
    /// </summary>
    public class EmailEntity
    {
        private Encoding subjectEncoding = Encoding.UTF8;
        private Encoding contentEncoding = Encoding.UTF8;
        private MailPriority priority = MailPriority.Normal;
        private string smtp = "smtp.qq.com";
        private int port = 25;
        private bool isSsl = true;

        /// <summary>
        /// 发件人邮箱地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 接收人邮件列表
        /// </summary>
        public List<string> ReceiverList { get; set; }

        /// <summary>
        /// 邮件标题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 邮件标题编码格式
        /// </summary>
        public Encoding SubjectEncoding
        {
            get { return this.subjectEncoding; }
            set { this.subjectEncoding = value; }
        }

        /// <summary>
        /// 邮件内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 邮件内容编码格式
        /// </summary>
        public Encoding ContentEncoding
        {
            get { return this.contentEncoding; }
            set { this.contentEncoding = value; }
        }

        /// <summary>
        /// 附件地址列表
        /// </summary>
        public List<string> AttachmentList { get; set; }

        /// <summary>
        /// 邮件优先级
        /// </summary>
        public MailPriority Priority
        {
            get { return this.priority; }
            set { this.priority = value; }
        }

        /// <summary>
        /// 发件人邮箱账户
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 发件人邮箱密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 邮箱 SMTP
        /// </summary>
        public string Smtp
        {
            get { return this.smtp; }
            set { this.smtp = value; }
        }

        /// <summary>
        /// 邮箱端口
        /// </summary>
        public int Port
        {
            get { return this.port; }
            set { this.port = value; }
        }

        /// <summary>
        /// 是否安全发送
        /// </summary>
        public bool IsSsl
        {
            get { return this.isSsl; }
            set { this.isSsl = value; }
        }

        public EmailEntity()
        {
            // 初始化设置
            this.SubjectEncoding = Encoding.UTF8;
        }
    }
    #endregion

    public class EmailHelper
    {
        #region 私有属性常量
        private static Dictionary<string, EmailSmtpData> SmtpDict = new Dictionary<string, EmailSmtpData>();
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 初始化邮件账户数据
        /// </summary>
        /// <param name="key">KEY</param>
        /// <param name="data">EmailSmtpData</param>
        public static void Init(string key, EmailSmtpData data)
        {
            if(SmtpDict.ContainsKey(key))
            {
                SmtpDict[key] = data;
            }
            else
            {
                SmtpDict.Add(key, data);
            }
        }
        /// <summary>
        /// 邮件发送
        /// </summary>
        /// <param name="key">KEY</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="content">邮件内容</param>
        /// <param name="receiverList">收件人地址列表</param>
        /// <returns></returns>
        public static bool Send(string key, string subject, string content, string[] receiverList)
        {
            if (!SmtpDict.ContainsKey(key)) return false;

            EmailSmtpData smtpData = SmtpDict[key];
            return Send(new EmailEntity()
            {
                Address = smtpData.Address,
                ReceiverList = receiverList.ToList<string>(),
                Subject = subject,
                Content = content,
                Account = smtpData.Account,
                Password = smtpData.Password,
                Smtp = smtpData.Smtp,
                Port = smtpData.Port,
                IsSsl = smtpData.IsSsl
            });
        }
        /// <summary>
        /// 邮件发送
        /// </summary>
        /// <param name="sendAddress">发件人地址</param>
        /// <param name="receiverAddress">收件人地址</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="content">邮件内容</param>
        /// <param name="account">发件人帐号</param>
        /// <param name="accountPassword">发件人密码</param>
        /// <param name="smtp">Smtp</param>
        /// <param name="smtpPort">Smtp 端口</param>
        /// <param name="isSsl">是否安全发送</param>
        /// <returns></returns>
        public static bool Send(string sendAddress, string receiverAddress, string subject, string content, string account, string accountPassword, string smtp = "smtp.qq.com", int smtpPort = 25, bool isSsl = true)
        {
            EmailEntity entity = new EmailEntity()
            {
                Address = sendAddress,
                ReceiverList = new List<string>() { receiverAddress },
                Subject = subject,
                Content = content,
                Account = account,
                Password = accountPassword,
                Smtp = smtp,
                Port = smtpPort,
                IsSsl = isSsl
            };
            return Send(entity);
        }
        /// <summary>
        /// 邮件发送
        /// </summary>
        /// <param name="emailEntity">EmailEntity</param>
        /// <returns></returns>
        public static bool Send(EmailEntity emailEntity)
        {
            SmtpClient client = null;
            try
            {
                List<string> receiverList = emailEntity.ReceiverList;
                List<string> attachmentList = emailEntity.AttachmentList;

                MailMessage mailMessage = new MailMessage();
                // 设置发件人
                mailMessage.From = new MailAddress(emailEntity.Address);
                // 设置收件人
                if (receiverList != null && receiverList.Count > 0)
                {
                    foreach (string receiver in receiverList)
                    {
                        mailMessage.To.Add(receiver);
                    }
                }
                // 设置邮件标题
                mailMessage.Subject = emailEntity.Subject;
                mailMessage.SubjectEncoding = emailEntity.SubjectEncoding;
                // 设置邮件内容
                mailMessage.Body = emailEntity.Content;
                mailMessage.BodyEncoding = emailEntity.ContentEncoding;
                // 设置附件
                if (attachmentList != null && attachmentList.Count > 0)
                {
                    foreach (string attachment in attachmentList)
                    {
                        mailMessage.Attachments.Add(new Attachment(attachment));
                    }
                }
                // 设置优先级
                mailMessage.Priority = emailEntity.Priority;
                // 设置邮件发送服务器
                client = new SmtpClient(emailEntity.Smtp, emailEntity.Port);
                // 设置发件人邮箱账号和密码
                client.Credentials = new NetworkCredential(emailEntity.Account, emailEntity.Password);
                // 是否启用安全发送
                client.EnableSsl = emailEntity.IsSsl;
                // 发送邮件
                client.Send(mailMessage);
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (client != null) client.Dispose();
            }
        }
        #endregion
    }

    #region 逻辑处理辅助类
    public class EmailSmtpData
    {
        /// <summary>
        /// 发件人邮件地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// SMTP
        /// </summary>
        public string Smtp { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 发件人账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 发件人密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// IsSsl
        /// </summary>
        public bool IsSsl { get; set; }
    }
    #endregion
}
