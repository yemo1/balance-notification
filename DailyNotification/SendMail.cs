using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DailyNotification
{
    public class SendMail
    {
        protected static string content = string.Empty;
        protected static string displayName = string.Empty;
        protected static string subject = string.Empty;
        protected static string config = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;

        public static void sendEmail(string to, string message)
        {
            //var config = ConfigurationManager.ConnectionStrings["local"].ConnectionString;
            //string displayName = string.Empty; //ConfigurationManager.AppSettings["sender"].ToString();
            //string subject = string.Empty;
            string from = ConfigurationManager.AppSettings["from"].ToString();
            string bcc = ConfigurationManager.AppSettings["bcc"].ToString();

            using (SmtpClient client = new SmtpClient())
            {
                MailAddress sender = new MailAddress(from, displayName);
                //MailAddress receiver = new MailAddress(to);
                using (MailMessage msg = new MailMessage())
                {
                    msg.From = sender;
                    msg.To.Add(to);
                    msg.Bcc.Add(bcc);
                    msg.Subject = subject;
                    msg.IsBodyHtml = true;
                    msg.Body = message;
                    client.Send(msg);
                }
            }
        }

        public static void sendEmail(string to, string message, string sub)
        {
            //var config = ConfigurationManager.ConnectionStrings["local"].ConnectionString;
            //string displayName = string.Empty; //ConfigurationManager.AppSettings["sender"].ToString();
            //string subject = string.Empty;
            string from = ConfigurationManager.AppSettings["from"].ToString();
            string bcc = ConfigurationManager.AppSettings["bcc"].ToString();

            using (SmtpClient client = new SmtpClient())
            {
                MailAddress sender = new MailAddress(from, "no-reply@gmail.com");
                //MailAddress receiver = new MailAddress(to);
                using (MailMessage msg = new MailMessage())
                {
                    msg.From = sender;
                    msg.To.Add(to);
                    msg.Bcc.Add(bcc);
                    msg.Subject = sub;
                    msg.IsBodyHtml = true;
                    msg.Body = message;
                    client.Send(msg);
                }
            }
        }
        public static string createEmailBody(string client, string suspend, string balance, string threshold, string clientid, string over_threshold)
        {

            //string content = string.Empty;
            ////using streamreader for reading my htmltemplate   

            //using (StreamReader reader = new StreamReader(@".\File\emailText.txt", Encoding.UTF8))
            //{
            //    content = reader.ReadToEnd();
            //}
            string qry = "select module,sender,subject,content from email_template where module=@module";
            //string content = string.Empty;
            using (MySqlConnection conn = new MySqlConnection(config))
            {
                conn.Open();
                using (MySqlCommand cd = new MySqlCommand(qry, conn))
                {
                    cd.Parameters.AddWithValue("@module", "balance_notification");
                    using (MySqlDataReader rd = cd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            if (rd.HasRows)
                            {
                                displayName = rd["sender"].ToString();
                                subject = rd["subject"].ToString();
                                content = rd["content"].ToString();
                            }
                        }
                    }
                }
            }

            content = content.Replace("{client}", client); //replacing the required things  

            content = content.Replace("{suspend}", suspend);

            content = content.Replace("{balance}", balance);
            content = content.Replace("{clientid}", clientid);
            content = content.Replace("{threshold}", threshold);
            content = content.Replace("{over_threshold}", over_threshold);
            content = content.Replace("{today}", string.Format("{0:dddd, MMMM d, yyyy}", DateTime.Now));

            return content;

        }
    }
}
