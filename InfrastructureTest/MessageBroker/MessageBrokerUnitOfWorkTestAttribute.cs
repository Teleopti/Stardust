using System;
using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder,configuration);

			builder.RegisterModule<MessageBrokerServerModule>();
		}

		protected override void BeforeTest()
		{
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnBeforeInvocation(null);
		}
		
		protected override void AfterTest()
		{
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnAfterInvocation(new cancelPersistException(), null);
		}

		private class cancelPersistException : Exception
		{
			
		}
	}
}