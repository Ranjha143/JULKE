using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace JULKE
{
    internal class GenerateRPAuth : IJob
    {
        readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public GenerateRPAuth()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Delay(0);
            threadWorker.RunWorkerAsync();
        }
        private void ThreadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var workerTask = Task.Factory.StartNew(() => ProccessQueue().Wait());
            Task.WaitAll(workerTask);
        }
        private void ThreadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        private void ThreadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        private async Task<bool> ProccessQueue()
        {

            try
            {

                var prismUser = ConfigurationManager.AppSettings["prismUser"].ToString();
                var prismPassword = ConfigurationManager.AppSettings["prismPassword"].ToString();

                await Task.Delay(0);
                AppVariables.RetailProAuthSession = RetailProAuthentication.GetSession(prismUser, prismPassword);
                File.AppendAllText("RetailProAuthSession.log", $"{DateTime.Now}: {AppVariables.RetailProAuthSession}");
            }
            catch (Exception)
            {

            }
            return true;
        }

    }

}