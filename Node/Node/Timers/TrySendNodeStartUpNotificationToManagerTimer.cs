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
    public class TrySendNodeStartUpNotificationToManagerTimer : Timer
    {
       
        public TrySendNodeStartUpNotificationToManagerTimer(INodeConfiguration nodeConfiguration,
                                                            Uri callbackTemplateUri,
                                                            double interval = 3000) : base(interval)
        {
            nodeConfiguration.ThrowArgumentNullException();
            callbackTemplateUri.ThrowArgumentNullExceptionWhenNull();

            NodeConfiguration = nodeConfiguration;
            CallbackTemplateUri = callbackTemplateUri;

            WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

            Elapsed += OnTimedEvent;

            AutoReset = true;
        }

        public string WhoAmI { get; private set; }

        public INodeConfiguration NodeConfiguration { get; private set; }

        public Uri CallbackTemplateUri { get; private set; }

        public event EventHandler TrySendNodeStartUpNotificationSucceded;

        public virtual async Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress)
        {
            var httpResponseMessage =
                await nodeAddress.PostAsync(CallbackTemplateUri);

            return httpResponseMessage;
        }

        private void TrySendNodeStartUpNotificationSuccededInvoke()
        {
            if (TrySendNodeStartUpNotificationSucceded != null)
            {
                TrySendNodeStartUpNotificationSucceded(this,
                                                       EventArgs.Empty);
            }
        }

        private async void OnTimedEvent(object sender,
                                        ElapsedEventArgs e)
        {
            try
            {
                LogHelper.LogInfoWithLineNumber("Trying to send init to manager");
                var httpResponseMessage =
                    await TrySendNodeStartUpToManager(NodeConfiguration.BaseAddress);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    LogHelper.LogDebugWithLineNumber(WhoAmI + ": Node start up notification to manager succeded. Manager Uri =  " +
                                     CallbackTemplateUri);

                    TrySendNodeStartUpNotificationSuccededInvoke();
                }
                else
                {
                    LogHelper.LogWarningWithLineNumber(WhoAmI + ": Node start up notification to manager failed. Error message =  " +
                                    httpResponseMessage.Content);
                }
            }

            catch 
            {
                LogHelper.LogErrorWithLineNumber(WhoAmI + ": Node start up notification to manager failed.");
            }
        }
    }
}