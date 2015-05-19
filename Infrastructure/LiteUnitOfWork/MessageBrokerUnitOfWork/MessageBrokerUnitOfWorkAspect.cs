using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork
{
	public class MessageBrokerUnitOfWorkAspect : IMessageBrokerUnitOfWorkAspect
	{
		private readonly IMessageBrokerUnitOfWorkScope _scope;

		public MessageBrokerUnitOfWorkAspect(IMessageBrokerUnitOfWorkScope scope)
		{
			_scope = scope;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_scope.Start();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.End(exception);
		}

	}
}