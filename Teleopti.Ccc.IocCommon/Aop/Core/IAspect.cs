using System;

namespace Teleopti.Ccc.IocCommon.Aop.Core
{
	public interface IAspect
	{
		void OnBeforeInvokation();
		void OnAfterInvokation(Exception exception);
	}
}