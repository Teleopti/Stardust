using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendNodeStartUpNotificationToManagerTimer : Timer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendNodeStartUpNotificationToManagerTimer));

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
                var httpResponseMessage =
                    await TrySendNodeStartUpToManager(NodeConfiguration.BaseAddress);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(WhoAmI + ": Node start up notification to manager succeded. Manager Uri =  " +
                                     CallbackTemplateUri);
                    }

                    TrySendNodeStartUpNotificationSuccededInvoke();
                }
                else
                {
                    if (Logger.IsWarnEnabled)
                    {
                        Logger.Warn(WhoAmI + ": Node start up notification to manager failed. Error message =  " +
                                    httpResponseMessage.Content);
                    }
                }
            }

            catch 
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(WhoAmI + ": Node start up notification to manager failed.");
                }
            }
        }
    }
}