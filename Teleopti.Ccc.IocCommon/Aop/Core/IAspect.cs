namespace Teleopti.Ccc.IocCommon.Aop.Core
{
	public interface IAspect
	{
		void OnBeforeInvokation();
		void OnAfterInvokation();
	}
}