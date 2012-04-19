namespace Teleopti.Ccc.Web.Core.Aop.Core
{
	public interface IAspect
	{
		void OnBeforeInvokation();
		void OnAfterInvokation();
	}
}