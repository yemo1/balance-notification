using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
//using System.Timers;
using System.Threading;
using System.Data;

namespace AutoMail.Classes
{
    public  class RunService
    {
        //private System.Threading.Timer timer;
        //private static TaskScheduler _instance;
        //private List<Timer> timers = new List<Timer>();
        //static System.Timers.Timer timer;

        //System.Timers.Timer time;
        //System.Timers.Timer tmrRestart = new System.Timers.Timer();
        //public RunService()
        //{
        //    time = new System.Timers.Timer(interval60Minutes);
        //}

        //const double interval60Minutes = 60 * 1000; // milliseconds to one hour


        public  void fetch()
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            if ((today != DayOfWeek.Saturday) && (today != DayOfWeek.Sunday))
            {
                var config = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;
               
                using (MySqlConnection conn = new MySqlConnection(config))
                {
                    string clientid = string.Empty;
                    string clientName = string.Empty;
                    string email = string.Empty;
                    string msg = string.Empty;
                    string currencySymbol = string.Empty;
                    string currencyCode = string.Empty;
                    string currencyBalance = string.Empty;
                    string thresholdValue = string.Empty;
                    string over_threshold_value = string.Empty;
                    DateTime suspension_date; //= string.Empty;
                    string suspend = string.Empty;
                    decimal balance = 0m;
                    decimal threshold = 0m;
                    decimal over_threshold = 0m;
                    string qry = "SELECT client_id,name,currency FROM client ";
                    string qry2 = "SELECT c.client_id,e.email, c.balance, c.threshold_amount, c.balance_date, c.suspension_date from client_suspension c join client_suspension_email e ON e.client_id = c.client_id WHERE balance > threshold_amount and e.client_id=@clientid";
                    int i, j;
                    MySqlDataAdapter da = new MySqlDataAdapter(qry, conn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "client");
                    StringBuilder b = new StringBuilder();
                    List<string> emails = new List<string>();
                    if(ds.Tables["client"].Rows.Count > 0)
                    {
                        for (i = 0; i < ds.Tables["client"].Rows.Count; i++)
                        {
                            clientid = ds.Tables["client"].Rows[i]["client_id"].ToString();
                            clientName = ds.Tables["client"].Rows[i]["name"].ToString();
                            currencyCode = ds.Tables["client"].Rows[i]["currency"].ToString();
                            MySqlCommand cmd = new MySqlCommand(qry2, conn);
                            cmd.Parameters.AddWithValue("@clientid", clientid);
                            MySqlDataAdapter mda = new MySqlDataAdapter(cmd);
                            DataSet dst = new DataSet();
                            mda.Fill(dst, "client_suspension_email");

                            if (dst.Tables["client_suspension_email"].Rows.Count > 0)
                            {
                                for (j = 0; j < dst.Tables["client_suspension_email"].Rows.Count; j++)
                                {
                                    emails.Add(dst.Tables["client_suspension_email"].Rows[j]["email"].ToString());
                                    suspension_date = DateTime.Parse(dst.Tables["client_suspension_email"].Rows[j]["suspension_date"].ToString());
                                    suspend = string.Format("{0:dddd, MMMM d, yyyy}", suspension_date);
                                    balance = Convert.ToDecimal(dst.Tables["client_suspension_email"].Rows[j]["balance"].ToString());
                                    threshold = Convert.ToDecimal(dst.Tables["client_suspension_email"].Rows[j]["threshold_amount"].ToString());
                                    over_threshold = balance - threshold;
                                    currencySymbol = CurrencyCodeMapper.GetSymbol(currencyCode);
                                    currencyBalance = currencySymbol == "?" ? string.Concat(currencyCode, balance.ToString("N")) : string.Concat(currencySymbol, balance.ToString("N"));
                                    thresholdValue = currencySymbol == "?" ? string.Concat(currencyCode, threshold.ToString("N")) : string.Concat(currencySymbol, threshold.ToString("N"));
                                    over_threshold_value = currencySymbol == "?" ? string.Concat("(",currencyCode, over_threshold.ToString("N"),")") : string.Concat("(",currencySymbol, over_threshold.ToString("N"),")");
                                }
                                email = string.Join(",", emails.ToArray());
                                msg = EmailSender.createEmailBody(clientName.ToUpper(), suspend, currencyBalance,thresholdValue,clientid, over_threshold_value);
                                EmailSender.sendEmail(email, msg);
                                emails.Clear();
                            }

                        }
                    }
                }
            }
        }

        

        public void Start()
        {
            int runHr = Convert.ToInt32(ConfigurationManager.AppSettings["startHr"].ToString());
            int runMin = Convert.ToInt32(ConfigurationManager.AppSettings["startMin"].ToString());
            double interval = Convert.ToDouble(ConfigurationManager.AppSettings["interval"].ToString());
            TaskScheduler.Instance.ScheduleTask(runHr,runMin,interval,fetch);

        }

        

        public bool Stop()
        {
            return false;
        }

   

        //==========================================//
        //private void SetUpTimer(TimeSpan alertTime)
        //{
        //    DateTime current = DateTime.UtcNow;
        //    TimeSpan timeToGo = alertTime - current.TimeOfDay;
        //    if (timeToGo < TimeSpan.Zero)
        //    {
        //        return;//time already passed
        //    }
        //    this.timer = new System.Threading.Timer(x =>
        //    {
        //        this.fetch();
        //    }, null, timeToGo, Timeout.InfiniteTimeSpan);
        //}


    }
}
