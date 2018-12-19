using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public interface IAspect
	{
		void OnBeforeInvocation(IInvocationInfo invocation);
		void OnAfterInvocation(Exception exception, IInvocationInfo invocation);
	}
}