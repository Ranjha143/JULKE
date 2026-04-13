using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace JULKE
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RetailProAuthSession();
            RetailPro_Shopify_SyncService();

        }
        private static async void RetailProAuthSession()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            IJobDetail job = JobBuilder.Create<GenerateRPAuth>()
                .WithIdentity("RetailProAuthSessionJob", "RetailProAuthSession")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("RetailProAuthSessionTrigger", "RetailProAuthSession")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }
        private static async void RetailPro_Shopify_SyncService()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            IJobDetail job = JobBuilder.Create<RetailPro_Shopify_SyncService>()
                .WithIdentity("RetailPro_Shopify_SyncServiceJob", "RetailPro_Shopify_SyncService")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("RetailPro_Shopify_SyncServiceTrigger", "RetailPro_Shopify_SyncService")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .RepeatForever())
                //.WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(23, 0,
                // DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                // DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }
    }

    public static class AppVariables
    {
        public static string ConnectionString { get; set; } = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST= {ConfigurationManager.AppSettings["DbServer"]})(PORT=1521)))" +
                      "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=RPROODS)));" +
                      "User Id = reportuser; " +
                      "Password = report; ";

        public static bool Shopify_FulfilmentInProgress { get; set; }
        public static bool Data_SyncService_InProgress { get; set; }
#if DEBUG
        public static string MongoConnectionString { get; set; } = $"{ConfigurationManager.AppSettings["MongoConnectionString"]}";

#endif
#if !DEBUG
  //public static string MongoConnectionString { get; set; } = "mongodb://admin:QndD7LgjOENy@185.197.250.149:27017/?authSource=admin";
  public static string MongoConnectionString { get; set; } = $"{ConfigurationManager.AppSettings["MongoConnectionString"]}";

#endif

        public static string MongoDatabase { get; set; } = "julke";
        public static string RetailProAuthSession { get; set; }

        //public static string ShopifyUserName = ConfigurationManager.AppSettings["ShopifyUserName"].ToString();
        //public static string ShopifyUserPassword = ConfigurationManager.AppSettings["ShopifyUserPassword"].ToString();
        //public static string ShopifyAccessToken = ConfigurationManager.AppSettings["ShopifyAccessToken"].ToString();

        //public static bool CourierServiceisAvailable { get; set; } = Convert.ToBoolean(ConfigurationManager.AppSettings["CourierServiceisAvailable"].ToString());



        //public static string BigCommerceAuthToken { get; set; } = "boichv42vvp7n97pv0ee4bxdug9czos";
        //public static string BigCommerceStoreHash { get; set; } = "4dv7h8u7im";
        //public static bool BigCommerceOrderCreationInProgress { get; set; }


    }
}
