using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAllBusinessUnitsUnitOfWorkAspect : IAllBusinessUnitsUnitOfWorkAspect
	{
		public bool Invoked;

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			Invoked = true;
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}