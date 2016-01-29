using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class NodeStartupNotificationToManagerFake : TrySendNodeStartUpNotificationToManagerTimer
    {
        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeStartupNotificationToManagerFake));

        public NodeStartupNotificationToManagerFake(INodeConfiguration nodeConfiguration ,
                                                    Uri callbackTemplateUri ,
                                                    double interval = 3000) : base(nodeConfiguration,
                                                                                   callbackTemplateUri,
                                                                                   interval)
        {
        }


        public override Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress)
        {
            Logger.Info("Try send node start up notification to manager.");

            Wait.Set();

            return null;
        }
    }
}