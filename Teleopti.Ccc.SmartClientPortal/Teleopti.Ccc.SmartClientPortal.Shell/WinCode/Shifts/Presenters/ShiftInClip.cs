using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
	//moved from ISessionData. This should be refactored into some real object instead of static data. But just leave it as it was before...
	public static class ShiftInClip
	{
		 public static IWorkShift Data { get; set; }
	}
}