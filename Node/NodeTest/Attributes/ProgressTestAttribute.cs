using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Autofac;
using NodeTest.Fakes;
using NodeTest.Fakes.Timers;
using Stardust.Node.API;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest.Attributes
{
    public class ProgressTestAttribute : BaseTestsAttribute
    {
        protected override void SetUp(ContainerBuilder builder)
        {
            var nodeConfiguration = new NodeConfiguration(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
                new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
                Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
                ConfigurationManager.AppSettings["NodeName"]);

            builder.RegisterAssemblyTypes(nodeConfiguration.HandlerAssembly)
                .Where(IsHandler)
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<InvokeHandler>();
            builder.RegisterType<SendJobFaultedTimerFake>()
                .SingleInstance();
            builder.RegisterType<SendJobCanceledTimerFake>()
                .SingleInstance();
            builder.RegisterType<SendJobDoneTimerFake>()
                .SingleInstance();

            builder.RegisterType<NodeController>();
            builder.RegisterType<PostHttpRequestFake>()
                .SingleInstance();

            builder.RegisterInstance(nodeConfiguration);

            // Register IWorkerWrapper.
            builder.Register<IWorkerWrapper>(c => new WorkerWrapper(c.Resolve<InvokeHandler>(),
                nodeConfiguration,
                new NodeStartupNotificationToManagerFake(),
                new PingToManagerFake(),
                c.Resolve<SendJobDoneTimerFake>(),
                c.Resolve<SendJobCanceledTimerFake>(),
                c.Resolve<SendJobFaultedTimerFake>(),
                c.Resolve<PostHttpRequestFake>()))
                .SingleInstance();
        }

        private bool IsHandler(Type arg)
        {
            return arg.GetInterfaces()
                .Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof (IHandle<>));
        }
    }
}