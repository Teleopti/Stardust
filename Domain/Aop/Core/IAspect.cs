using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public interface IAspect
	{
		void OnBeforeInvocation();
		void OnAfterInvocation(Exception exception);
	}
}