using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public interface IAspect
	{
		void OnBeforeInvokation();
		void OnAfterInvokation(Exception exception);
	}
}