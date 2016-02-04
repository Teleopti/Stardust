using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendStatusToManagerTimer : Timer
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (TrySendStatusToManagerTimer));

        public TrySendStatusToManagerTimer(INodeConfiguration nodeConfiguration,
                                           Uri callbackTemplateUri,
                                           double interval = 10000) : base(interval)
        {
            // Validate arguments.
            nodeConfiguration.ThrowArgumentNullException();
            callbackTemplateUri.ThrowArgumentNullExceptionWhenNull();

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
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 "Error in TrySendStatus.",
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
                        LogHelper.LogDebugWithLineNumber(Logger,
                                                         WhoAmI + ": Try send status to manager succeded. Send Uri =  " + httpResponseMessage.RequestMessage.RequestUri);


                        InvokeTriggerTrySendStatusSucceded();
                    }
                    else
                    {
                        LogHelper.LogWarningWithLineNumber(Logger,
                                                           WhoAmI + ": " + httpResponseMessage.ReasonPhrase);
                    }
                }
            }

            catch (Exception)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 WhoAmI + ": Try send status to manager failed.");
            }

            finally
            {
                Stop();
            }
        }
    }
}