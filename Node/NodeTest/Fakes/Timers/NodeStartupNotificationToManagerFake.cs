using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class NodeStartupNotificationToManagerFake : TrySendNodeStartUpNotificationToManagerTimer
    {
        public ManualResetEventSlim Wait = new ManualResetEventSlim();
        
        public NodeStartupNotificationToManagerFake(INodeConfiguration nodeConfiguration ,
                                                    Uri callbackTemplateUri ,
                                                    double interval = 3000) : base(nodeConfiguration,
                                                                                   callbackTemplateUri,
                                                                                   interval)
        {
        }


        public override Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress)
        {
            LogHelper.LogInfoWithLineNumber("Try send node start up notification to manager.");

            Wait.Set();

            return null;
        }
    }
}