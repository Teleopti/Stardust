using System;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeReadModelUnitOfWorkAspect : IReadModelUnitOfWorkAspect
	{
		public void OnBeforeInvocation()
		{
		}

		public void OnAfterInvocation(Exception exception)
		{
		}
	}
}