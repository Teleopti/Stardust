using System.Threading;
using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace Stardust.Node
{
	public class NodeModule : Module
	{
		//private readonly NodeConfiguration _nodeConfiguration;

		//public NodeModule(NodeConfiguration nodeConfiguration)
		//{
		//	_nodeConfiguration = nodeConfiguration;
		//}

		protected override void Load(ContainerBuilder builder)
		{
			//builder.RegisterInstance(_nodeConfiguration).As<NodeConfiguration>().SingleInstance();
			builder.RegisterType<HttpSender>().As<IHttpSender>().InstancePerDependency();
			builder.RegisterType<InvokeHandler>().As<IInvokeHandler>().InstancePerDependency();
			builder.RegisterType<WorkerWrapper>().As<IWorkerWrapper>().InstancePerDependency();

			builder.RegisterApiControllers(typeof (NodeController).Assembly);

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
	}
}