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

        public TrySendStatusToManagerTimer(INodeConfiguration nodeConfiguration,
                                           Uri callbackTemplateUri,
                                           double interval = 10000) : base(interval)
        {
            // Validate arguments.
            nodeConfiguration.ThrowArgumentNullException();
            callbackTemplateUri.ThrowArgumentExceptionWhenNull();

            // Assign values.
            NodeConfiguration = nodeConfiguration;

            WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

            CallbackTemplateUri = callbackTemplateUri;

            Elapsed += OnTimedEvent;

            AutoReset = true;
        }

        public string WhoAmI { get; private set; }

        public JobToDo JobToDo { get; set; }

        public INodeConfiguration NodeConfiguration { get; private set; }

        public Uri CallbackTemplateUri { get; set; }

        public event EventHandler TrySendStatusSucceded;

        public void InvokeTriggerTrySendStatusSucceded()
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
                    await jobToDo.PostAsync(CallbackTemplateUri);

                return httpResponseMessage;
            }

            catch (Exception exp)
            {
                Logger.Error("Error in TrySendStatus.",
                             exp);
                throw;
            }
        }

        private async void OnTimedEvent(object sender,
                                        ElapsedEventArgs e)
        {
            Stop();

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
                            Logger.Debug(WhoAmI + ": Try send status to manager succeded. Manager Uri =  " + CallbackTemplateUri);
                        }

                        InvokeTriggerTrySendStatusSucceded();
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

            finally
            {
                Stop();
            }
        }
    }
}