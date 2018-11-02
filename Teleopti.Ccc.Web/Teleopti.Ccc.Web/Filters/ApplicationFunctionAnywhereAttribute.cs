using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Web.Filters
{
	public sealed class AddFullDayAbsencePermissionAttribute : ApplicationFunctionApiAttribute
	{
		public AddFullDayAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence)
		{
		}
	}

	public sealed class AddIntradayAbsencePermissionAttribute : ApplicationFunctionApiAttribute
	{
		public AddIntradayAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence)
		{
		}
	}

	public sealed class SwapShiftPermissionAttribute : ApplicationFunctionApiAttribute
	{
		public SwapShiftPermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.SwapShifts)
		{
		}
	}
}