//using Autofac;
//using Autofac.Integration.WebApi;
using Autofac;
using Stardust.Core.Node.Interfaces;
using Stardust.Core.Node.Timers;
using Stardust.Core.Node.Workers;
using HttpSender = Stardust.Core.Node.Workers.HttpSender;
using WorkerWrapper = Stardust.Core.Node.Workers.WorkerWrapper;

namespace Stardust.Core.Node
{
	public class NodeModule : Module
	{

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpSender>().As<IHttpSender>().SingleInstance();
            builder.RegisterType<InvokeHandler>().As<IInvokeHandler>().InstancePerDependency();
            builder.RegisterType<WorkerWrapper>().As<IWorkerWrapper>().InstancePerDependency();

            //builder.RegisterApiControllers(typeof(NodeController).Assembly);

            builder.RegisterType<TrySendJobDetailToManagerTimer>().InstancePerDependency();
            builder.RegisterType<TrySendNodeStartUpNotificationToManagerTimer>().InstancePerDependency();
            builder.RegisterType<TrySendJobDoneStatusToManagerTimer>().InstancePerDependency();
            builder.RegisterType<PingToManagerTimer>().As<IPingToManagerTimer>().InstancePerDependency();
            builder.RegisterType<TrySendJobFaultedToManagerTimer>().InstancePerDependency();
            builder.RegisterType<TrySendJobCanceledToManagerTimer>().InstancePerDependency();

            builder.RegisterType<JobDetailSender>().InstancePerLifetimeScope();

            builder.RegisterType<Now>().As<INow>().SingleInstance();
            builder.RegisterType<WorkerWrapperService>().SingleInstance();
        }

        //public static void RegisterNodeServices(IServiceCollection services)
        //{
        //    services.AddSingleton<IHttpSender, HttpSender>();
        //    services.AddTransient<IInvokeHandler, InvokeHandler>();
        //    services.AddTransient<IWorkerWrapper, WorkerWrapper>();

        //    services.AddTransient<TrySendJobDetailToManagerTimer>();
        //    services.AddTransient<TrySendNodeStartUpNotificationToManagerTimer>();
        //    services.AddTransient<TrySendJobDoneStatusToManagerTimer>();
        //    services.AddTransient<PingToManagerTimer>();
        //    services.AddTransient<TrySendJobFaultedToManagerTimer>();
        //    services.AddTransient<TrySendJobCanceledToManagerTimer>();

        //    services.AddScoped<JobDetailSender>();

        //    services.AddSingleton<INow, Now>();
        //    services.AddSingleton<WorkerWrapperService>();
        //}
	}
}