using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	public class MessageBrokerServerTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule<MessageBrokerServerModule>();
			builder.RegisterInstance(new FakeSignalR()).AsSelf().As<ISignalR>();
			builder.RegisterInstance(new ActionImmediate()).As<IActionScheduler>();

			builder.RegisterInstance(new FakeCurrentDatasource()).AsSelf().As<ICurrentDataSource>();
			builder.RegisterInstance(new FakeCurrentBusinessUnit()).AsSelf().As<ICurrentBusinessUnit>();

		}
	}
}