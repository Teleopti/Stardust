using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class PingToManagerTimer : Timer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (PingToManagerTimer));

        public PingToManagerTimer(INodeConfiguration nodeConfiguration,
                                  Uri callbackUri,
                                  double interval = 10000) : base(interval)
        {
            nodeConfiguration.ThrowArgumentNullException();
            callbackUri.ThrowArgumentNullExceptionWhenNull();

            NodeConfiguration = nodeConfiguration;
            CallbackUri = callbackUri;

            WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

            Elapsed += OnTimedEvent;

            AutoReset = true;
        }

        public string WhoAmI { get; private set; }

        public INodeConfiguration NodeConfiguration { get; private set; }
        public Uri CallbackUri { get; private set; }


        public async Task<HttpResponseMessage> SendPing(Uri nodeAddress)
        {
            var httpResponseMessage =
                await nodeAddress.PostAsync(CallbackUri);

            return httpResponseMessage;
        }

        private async void OnTimedEvent(object sender,
                                        ElapsedEventArgs e)
        {
            try
            {
                await SendPing(NodeConfiguration.BaseAddress); 
            }
            catch 
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(WhoAmI + ": Heartbeat failed. Is the manager up and running?");
                }
            }
        }
    }
}