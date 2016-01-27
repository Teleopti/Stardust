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

        public TrySendNodeStartUpNotificationToManagerTimer(INodeConfiguration nodeConfiguration = null,
            Uri callbackUri = null,
            ElapsedEventHandler overrideElapsedEventHandler = null,
            double interval = 3000) : base(interval)
        {
            NodeConfiguration = nodeConfiguration;
            CallbackUri = callbackUri;

            WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

            OverrideElapsedEventHandler = overrideElapsedEventHandler;

            if (OverrideElapsedEventHandler != null)
            {
                Elapsed += OverrideElapsedEventHandler;
            }
            else
            {
                Elapsed += OnTimedEvent;
            }

            AutoReset = true;
        }

        public string WhoAmI { get; private set; }

        public INodeConfiguration NodeConfiguration { get; private set; }
        public Uri CallbackUri { get; private set; }
        public ElapsedEventHandler OverrideElapsedEventHandler { get; set; }
        public event EventHandler TrySendNodeStartUpNotificationSucceded;

        public async Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress)
        {
            var httpResponseMessage =
                await nodeAddress.PostAsync(CallbackUri);

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
                                     CallbackUri);
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

            catch (Exception exp)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(WhoAmI + ": Node start up notification to manager failed.", exp);
                }
            }
        }
    }
}