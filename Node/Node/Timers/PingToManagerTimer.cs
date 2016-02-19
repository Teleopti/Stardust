using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
    public class PingToManagerTimer : Timer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (PingToManagerTimer));

        public PingToManagerTimer(INodeConfiguration nodeConfiguration,
                                  Uri callbackUri,
                                  double interval = 60000) : base(interval)
        {
            nodeConfiguration.ThrowArgumentNullException();
            callbackUri.ThrowArgumentNullExceptionWhenNull();

            CancellationTokenSource = new CancellationTokenSource();

            NodeConfiguration = nodeConfiguration;
            CallbackUri = callbackUri;

            WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

            Elapsed += OnTimedEvent;

            AutoReset = true;
        }

        protected override void Dispose(bool disposing)
        {
            LogHelper.LogDebugWithLineNumber(Logger,"Start disposing.");

            if (CancellationTokenSource != null &&
                !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }

            base.Dispose(disposing);

            LogHelper.LogDebugWithLineNumber(Logger, "Finished disposing.");
        }

        public string WhoAmI { get; private set; }

        public INodeConfiguration NodeConfiguration { get; private set; }

        public Uri CallbackUri { get; private set; }


        private async Task<HttpResponseMessage> SendPing(Uri nodeAddress,
                                                         CancellationToken cancellationToken)
        {
            var httpResponseMessage =
                await nodeAddress.PostAsync(CallbackUri,
                                            cancellationToken);

            return httpResponseMessage;
        }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private async void OnTimedEvent(object sender,
                                        ElapsedEventArgs e)
        {
            try
            {
                await SendPing(NodeConfiguration.BaseAddress,
                               CancellationTokenSource.Token);
            }

            catch
            {
                LogHelper.LogInfoWithLineNumber(Logger,
                                                 WhoAmI + ": Heartbeat failed. Is the manager up and running?");
            }
        }
    }
}