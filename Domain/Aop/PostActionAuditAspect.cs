using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Domain.Aop
{
	public class PreActionAuditAspect : IAspect
	{
		private readonly IResolve _resolve;

		public PreActionAuditAspect(IResolve resolve)
		{
			_resolve = resolve;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var handlerType = typeof(IHandleContextAction<>).MakeGenericType(invocation.Arguments[0].GetType());
			var handler = _resolve.Resolve(handlerType);
			var method = handler.GetType().GetMethod("Handle", new[] { invocation.Arguments[0].GetType() });
			method.Invoke(handler, new[] { invocation.Arguments[0] });
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			
		}
	}

	public class PostActionAuditAspect : IAspect
	{
		private readonly IResolve _resolve;

		public PostActionAuditAspect(IResolve resolve)
		{
			_resolve = resolve;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{

		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (exception != null) return;
			//We can also introduce the correlatoin id here as well and pass in in the argument list but that means to chagne the IHandleContextAction Contract
			//using (var resolve = _resolve.NewScope())
			var handlerType = typeof(IHandleContextAction<>).MakeGenericType(invocation.Arguments[0].GetType());
			var handler = _resolve.Resolve(handlerType);
			var method = handler.GetType().GetMethod("Handle", new[] { invocation.Arguments[0].GetType() });
			method.Invoke(handler, new[] { invocation.Arguments[0] });
		}
	}
}