﻿using System;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.Owin.Hosting;
using Owin;
using Stardust.Node.API;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace Stardust.Node
{
    public class NodeStarter : INodeStarter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeStarter));

        private string WhoAmI { get; set; }

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        public void Stop()
        {
            QuitEvent.Set();
        }

        public void Start(INodeConfiguration nodeConfiguration,
                          IContainer container)
        {
            string nodeAddress = nodeConfiguration.BaseAddress.Scheme + "://+:" + nodeConfiguration.BaseAddress.Port + "/";
            // Start OWIN host 
            using (WebApp.Start(nodeAddress,
                                appBuilder =>
                                {
                                    var builder = new ContainerBuilder();

                                    builder.RegisterType<InvokeHandler>()
                                        .SingleInstance();

                                    builder.RegisterType<NodeController>()
                                        .SingleInstance();

                                    builder.RegisterApiControllers(typeof (NodeController).Assembly);

                                    builder.RegisterInstance(nodeConfiguration);

                                    // Register IWorkerWrapper.
                                    builder.Register<IWorkerWrapper>(c => new WorkerWrapper(c.Resolve<InvokeHandler>(),
                                                                                            nodeConfiguration,
                                                                                            new TrySendNodeStartUpNotificationToManagerTimer(nodeConfiguration,
                                                                                                                                             nodeConfiguration.GetManagerNodeHasBeenInitializedUri()),
                                                                                            new PingToManagerTimer(nodeConfiguration,
                                                                                                                   nodeConfiguration.GetManagerNodeHeartbeatUri()),
                                                                                            new TrySendJobDoneStatusToManagerTimer(nodeConfiguration),
                                                                                            new TrySendJobCanceledToManagerTimer(nodeConfiguration),
                                                                                            new TrySendJobFaultedToManagerTimer(nodeConfiguration),
                                                                                            new PostHttpRequest()))
                                        .SingleInstance();


                                    builder.Update(container);

                                    //to start it
                                    container.Resolve<IWorkerWrapper>();

                                    // Configure Web API for self-host. 
                                    var config = new HttpConfiguration
                                    {
                                        DependencyResolver = new AutofacWebApiDependencyResolver(container)
                                    };

                                    config.MapHttpAttributeRoutes();
                                    config.Services.Add(typeof (IExceptionLogger),
                                                        new GlobalExceptionLogger());

                                    appBuilder.UseAutofacMiddleware(container);
                                    appBuilder.UseAutofacWebApi(config);
                                    appBuilder.UseWebApi(config);
                                }))

            {
                WhoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);

                LogHelper.LogInfoWithLineNumber(Logger,
                                                WhoAmI + ": Node started on machine.");

                LogHelper.LogInfoWithLineNumber(Logger,
                                                WhoAmI + ": Listening on port " + nodeConfiguration.BaseAddress);

                QuitEvent.WaitOne();
            }
        }
    }
}