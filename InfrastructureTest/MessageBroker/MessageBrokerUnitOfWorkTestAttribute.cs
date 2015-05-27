using System;
using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder,configuration);

			builder.RegisterType<MailboxRepository>()
				.As<IMailboxRepository>()
				.ApplyAspects()
				.SingleInstance();
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