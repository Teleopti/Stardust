using System;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeAllBusinessUnitsUnitOfWorkAspect : IAllBusinessUnitsUnitOfWorkAspect
	{
		public bool Invoked;

		public void OnBeforeInvocation()
		{
			Invoked = true;
		}

		public void OnAfterInvocation(Exception exception)
		{
		}
	}
}