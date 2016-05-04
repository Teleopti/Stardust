using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace Stardust.Node
{
	public class NodeModule : Module
	{
		private readonly NodeConfiguration _nodeConfiguration;

		public NodeModule(NodeConfiguration nodeConfiguration)
		{
			_nodeConfiguration = nodeConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<HttpSender>().As<IHttpSender>().SingleInstance();
			builder.RegisterInstance(_nodeConfiguration).As<NodeConfiguration>().SingleInstance();
			builder.RegisterType<InvokeHandler>().As<IInvokeHandler>().SingleInstance();
			builder.RegisterApiControllers(typeof (NodeController).Assembly);

			builder.RegisterType<TrySendJobDetailToManagerTimer>().SingleInstance();
			builder.RegisterType<TrySendNodeStartUpNotificationToManagerTimer>().SingleInstance();
			builder.RegisterType<TrySendJobDoneStatusToManagerTimer>().SingleInstance();
			builder.RegisterType<PingToManagerTimer>().As<System.Timers.Timer>().SingleInstance();
			builder.RegisterType<TrySendJobFaultedToManagerTimer>().SingleInstance();
			builder.RegisterType<TrySendJobCanceledToManagerTimer>().SingleInstance();
			builder.RegisterType<WorkerWrapper>().As<IWorkerWrapper>().SingleInstance();
		}
	}
}