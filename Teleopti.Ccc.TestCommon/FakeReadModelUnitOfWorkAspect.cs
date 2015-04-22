using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeReadModelUnitOfWorkAspect : IReadModelUnitOfWorkAspect
	{
		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}