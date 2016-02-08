﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class SendJobCanceledTimerFake : TrySendStatusToManagerTimer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SendJobCanceledTimerFake));

        public int NumberOfTimeCalled;

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public SendJobCanceledTimerFake(INodeConfiguration nodeConfiguration,
                                        Uri callbackTemplateUri,
                                        double interval = 10000) : base(nodeConfiguration,
                                                                        callbackTemplateUri,
                                                                        interval)
        {
        }

        public override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
        {
            Wait.Set();

            NumberOfTimeCalled++;

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var request = new HttpRequestMessage(HttpMethod.Post, CallbackTemplateUri);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}