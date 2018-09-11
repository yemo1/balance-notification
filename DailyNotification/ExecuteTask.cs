using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyNotification
{
    public class ExecuteTask
    {
        private static int runHr;
        private static int runMin;
        private static int runSec;
        private static double interval;
        private static string clientid;
        protected static string clientName;
        public string runPeriod;
        private static string clientMail;
        private static decimal balance;
        private static string currencyCode;
        private static string currencySymbol;
        private static string currencyBalance;
        private static string country_name;
        private static string network_name;
        private static string mccmnc;
        private static decimal charge;
        private static decimal price;
        private static int num;
        private static int nosms;
        private static decimal totalCost;
        private static string msg;
        private static string qry;
        private static string qry2;
        private static string content;

        public void getData()
        {
            StringBuilder htmlTable = new StringBuilder();
            var config = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(config))
            {
                 qry = "SELECT client_id,name,email,currency,balance,balance_notification FROM client ";
                 qry2 = @"Select c.name Country,n.name Network,m.mccmnc MCCMNC,m.charge Charge,m.unit_price Price,count(*) Num,sum(nosms) 'Count' 
                                    from message m
                                    join network n on n.mccmnc = m.mccmnc
                                    join countries c on c.code = n.country_code
                                    where m.client_id = @clientid and m.status in ('ABS', 'DLG', 'DLV', 'ERD', 'USB') and m.charge > 0 
                                    group by m.mccmnc";
                
                int i, j;
                //int runHr, runMin, runSec;
                //double interval;
                MySqlDataAdapter da = new MySqlDataAdapter(qry, conn);
                DataSet ds = new DataSet();
                da.Fill(ds, "client");
                StringBuilder b = new StringBuilder();
                List<string> emails = new List<string>();
                if (ds.Tables["client"].Rows.Count > 0)
                {
                    for (i = 0; i < ds.Tables["client"].Rows.Count; i++)
                    {
                        int total = 0;
                        int numsms = 0;
                        decimal totalPrice = 0m;
                        clientid = ds.Tables["client"].Rows[i]["client_id"].ToString();
                        clientName = ds.Tables["client"].Rows[i]["name"].ToString();
                        clientMail = ds.Tables["client"].Rows[i]["email"].ToString();
                        if(ds.Tables["client"].Rows[i]["balance"] != DBNull.Value)
                        {
                            balance = Convert.ToDecimal(ds.Tables["client"].Rows[i]["balance"].ToString());
                            currencyCode = ds.Tables["client"].Rows[i]["currency"].ToString();
                            currencySymbol = CurrencyMapper.GetSymbol(currencyCode);
                        }
                       
                        currencyBalance = currencySymbol == "?" ? string.Concat(currencyCode, balance.ToString("N")) : string.Concat(currencySymbol, balance.ToString("N"));
                        runPeriod = ds.Tables["client"].Rows[i]["balance_notification"].ToString().ToLower();

                        if (runPeriod == "daily")
                        {
                            MySqlCommand cmd = new MySqlCommand(qry2, conn);
                            cmd.Parameters.AddWithValue("@clientid", clientid);
                            MySqlDataAdapter mda = new MySqlDataAdapter(cmd);
                            DataSet dst = new DataSet();
                            mda.Fill(dst, "message");
                            htmlTable = new StringBuilder();
                            content = string.Concat("Dear ", "<b>", clientName, "</b>", "<br/><br/>", "Your balance as of ", string.Format("{0:dd-MM-yyy}", DateTime.Now), " is ", currencyBalance, "<br/>", "Details of sms sent today is shown below", "<br/><br/>");

                            htmlTable.Append("<table border='0' align='center' width='60%' cellspacing='0'>");
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td><img src='http://wirepick.com/assets/img/logo.png'></td>");
                            htmlTable.Append("</tr>");
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td colspan='8'><hr style='border-top: 2px solid #3498DB;'></td>");
                            htmlTable.Append("</tr>");
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td  colspan = '8'>" + content + "</td>");
                            htmlTable.Append("</tr>");
                            htmlTable.Append("<tr style='background-color:#395870; color:White;'><th>Country</th><th>Network</th><th>MCCMNC</th><th>Charge</th><th>Price</th><th>Message</th><th>No of SMS</th><th>Total Cost</th></tr>");
                            if (dst.Tables["message"].Rows.Count > 0)
                            {
                                for (j = 0; j < dst.Tables["message"].Rows.Count; j++)
                                {
                                    country_name = dst.Tables["message"].Rows[j]["Country"].ToString();
                                    network_name = dst.Tables["message"].Rows[j]["Network"].ToString();
                                    mccmnc = dst.Tables["message"].Rows[j]["MCCMNC"].ToString();
                                    charge = Convert.ToDecimal(dst.Tables["message"].Rows[j]["Charge"].ToString());
                                    price = Convert.ToDecimal(dst.Tables["message"].Rows[j]["Price"].ToString());
                                    num = Convert.ToInt32(dst.Tables["message"].Rows[j]["Num"].ToString());
                                    nosms = Convert.ToInt32(dst.Tables["message"].Rows[j]["Count"].ToString());
                                    totalCost = Convert.ToDecimal(nosms * charge);


                                    htmlTable.Append("<tr align='center' style='color: Black;'>");
                                    htmlTable.Append("<td>" + country_name + "</td>");
                                    htmlTable.Append("<td>" + network_name + "</td>");
                                    htmlTable.Append("<td>" + mccmnc + "</td>");
                                    htmlTable.Append("<td>" + charge + "</td>");
                                    htmlTable.Append("<td>" + price + "</td>");
                                    htmlTable.Append("<td>" + num + "</td>");
                                    htmlTable.Append("<td>" + nosms + "</td>");
                                    htmlTable.Append("<td>" + totalCost + "</td>");

                                    totalPrice += totalCost;
                                    total += num;
                                    numsms += nosms;
                                }
                                htmlTable.Append("</tr>");
                                htmlTable.Append("<tr style='background-color: #395870; color: White'>");
                                htmlTable.Append("<th colspan='5'>Total</th>");
                                htmlTable.Append("<th>" + total + "</th>");
                                htmlTable.Append("<th>" + numsms + "</th>");
                                htmlTable.Append("<th>" + totalPrice + "</th>");
                                htmlTable.Append("</tr>");
                                htmlTable.Append("</table>");
                                msg = htmlTable.ToString();
                                SendMail.sendEmail(clientMail, msg, "Daily SMS Usage");
                            }
                            //SendMail.sendEmail(clientMail, msg, "Daily SMS Usage");
                        }
                    }
                }
                //SendMail.sendEmail(clientMail, msg, "Daily SMS Usage");
            }
        }

        public void Start()
        {
            //var config = ConfigurationManager.ConnectionStrings["local"].ConnectionString;
            //string period = string.Empty;
            //int Hr=0, Min=0, Sec=0, wHr=0, wMin=0, wSec=0;
            //double wintvl=0, intvl=0;

            //using (MySqlConnection conn = new MySqlConnection(config))
            //{
            //    string query = "SELECT balance_notification FROM client";

            //    int i,j;
            //    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
            //    DataSet ds = new DataSet();
            //    da.Fill(ds, "client");
            //    if (ds.Tables["client"].Rows.Count > 0)
            //    {
            //        for (i = 0; i < ds.Tables["client"].Rows.Count; i++)
            //        {
            //            period = ds.Tables["client"].Rows[i]["balance_notification"].ToString().ToLower();


            //            if (period == "daily")
            //            {
            //                Hr = Convert.ToInt32(ConfigurationManager.AppSettings["dailyHr"].ToString());
            //                Min = Convert.ToInt32(ConfigurationManager.AppSettings["dailyMin"].ToString());
            //                Sec = Convert.ToInt32(ConfigurationManager.AppSettings["dailySec"].ToString());
            //                intvl = Convert.ToDouble(ConfigurationManager.AppSettings["interval"].ToString());
            //                ScheduleTask.Instance.TaskScheduler(Hr, Min, intvl, getData);
            //            }
            //            if (period == "weekly")
            //            {
            //                wHr = Convert.ToInt32(ConfigurationManager.AppSettings["weeklyHr"].ToString());
            //                wMin = Convert.ToInt32(ConfigurationManager.AppSettings["weeklyMin"].ToString());
            //                wSec = Convert.ToInt32(ConfigurationManager.AppSettings["weeklySec"].ToString());
            //                wintvl = Convert.ToDouble(ConfigurationManager.AppSettings["winterval"].ToString());
            //                //ScheduleTask.Instance.TaskScheduler(wHr, wMin, wintvl, getData);
            //            }
            //        }

            //    }

            //}
            int runHr = Convert.ToInt32(ConfigurationManager.AppSettings["dailyHr"].ToString());
            int runMin = Convert.ToInt32(ConfigurationManager.AppSettings["dailyMin"].ToString());
            double interval = Convert.ToDouble(ConfigurationManager.AppSettings["interval"].ToString());
            ScheduleTask.Instance.TaskScheduler(runHr, runMin, interval, getData);
            //ScheduleTask.Instance.TaskScheduler(wHr, wMin, wintvl, getData);
        }

        public void Stop()
        {

        }
    }
}
