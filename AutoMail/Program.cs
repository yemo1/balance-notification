using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using AutoMail.Classes;
using Topshelf;

namespace AutoMail
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig =>
            {
                serviceConfig.Service<RunService>(serviceInstance =>
                {
                    serviceInstance.ConstructUsing(() => new RunService());
                    serviceInstance.WhenStarted(execute => execute.Start());
                    serviceInstance.WhenStopped(execute => execute.Stop());
                });
                serviceConfig.SetServiceName("CreditBalanceNotifyer");
                serviceConfig.SetDisplayName("Credit Balance Notifyer");
                serviceConfig.SetDescription("Credit Balance Notification Service");
                serviceConfig.StartAutomatically();
            });
        }
    }
}
