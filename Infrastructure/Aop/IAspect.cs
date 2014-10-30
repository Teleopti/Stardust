using System;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public interface IAspect
	{
		void OnBeforeInvokation();
		void OnAfterInvokation(Exception exception);
	}
}