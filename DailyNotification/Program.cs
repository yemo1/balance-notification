using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace DailyNotification
{
    class Program
    {
        static void Main(string[] args)
        {
            //ExecuteTask.getData();
            HostFactory.Run(serviceConfig =>
            {
                serviceConfig.Service<ExecuteTask>(serviceInstance =>
                {
                    serviceInstance.ConstructUsing(() => new ExecuteTask());
                    serviceInstance.WhenStarted(execute => execute.Start());
                    serviceInstance.WhenStopped(execute => execute.Stop());
                });
                serviceConfig.SetServiceName("SmsUsageNotifier");
                serviceConfig.SetDisplayName("SMS Usage Notification");
                serviceConfig.SetDescription("Send credit balance and daily sms usage notification");
                serviceConfig.StartAutomatically();
            });

        }
    }
}
