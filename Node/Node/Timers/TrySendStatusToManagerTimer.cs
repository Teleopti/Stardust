using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendStatusToManagerTimer : Timer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendStatusToManagerTimer));

        public TrySendStatusToManagerTimer(JobToDo jobToDo,
                                           INodeConfiguration nodeConfiguration,
                                           Uri callbackUri,
                                           ElapsedEventHandler overrideElapsedEventHandler = null,
                                           double interval = 10000) : base(interval)
        {
            JobToDo = jobToDo;

            NodeConfiguration = nodeConfiguration;

            WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

            CallbackUri = callbackUri;
            OverrideElapsedEventHandler = overrideElapsedEventHandler;

            if (overrideElapsedEventHandler != null)
            {
                Elapsed += overrideElapsedEventHandler;
            }
            else
            {
                Elapsed += OnTimedEvent;
            }

            AutoReset = true;
        }

        public string WhoAmI { get; private set; }

        public JobToDo JobToDo { get; set; }
        public INodeConfiguration NodeConfiguration { get; private set; }
        public Uri CallbackUri { get; set; }

        private ElapsedEventHandler OverrideElapsedEventHandler { get; set; }

        public event EventHandler TrySendStatusSucceded;

        public void TriggerTrySendStatusSucceded()
        {
            if (TrySendStatusSucceded != null)
            {
                TrySendStatusSucceded(this,
                                      EventArgs.Empty);
            }
        }

        public virtual async Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
        {
            try
            {
                var httpResponseMessage =
                    await jobToDo.PostAsync(CallbackUri);

                return httpResponseMessage;

            }

            catch (Exception exp)
            {
                Logger.Error("Error in TrySendStatus.",exp);
                throw;
            }
        }

        private async void OnTimedEvent(object sender,
                                        ElapsedEventArgs e)
        {
            try
            {
                if (JobToDo != null)
                {
                    var httpResponseMessage =
                        await TrySendStatus(new JobToDo
                        {
                            Id = JobToDo.Id
                        });

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug(WhoAmI + ": Try send status to manager succeded. Manager Uri =  " + CallbackUri);
                        }

                        TriggerTrySendStatusSucceded();
                    }
                    else
                    {
                        if (Logger.IsWarnEnabled)
                        {
                            Logger.Warn(WhoAmI + ": " + httpResponseMessage.ReasonPhrase);
                        }
                    }
                }
            }

            catch (Exception)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(WhoAmI + ": Try send status to manager failed.");
                }
            }

        }
    }
}