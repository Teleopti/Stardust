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

	public sealed class RemoveAbsencePermissionAttribute : ApplicationFunctionApiAttribute
	{
		public RemoveAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.RemoveAbsence)
		{
		}
	}

	public sealed class AddActivityPermissionAttribute : ApplicationFunctionApiAttribute
	{
		public AddActivityPermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddActivity)
		{
		}
	}

	public sealed class MoveActivityPermissionAttribute : ApplicationFunctionApiAttribute
	{
		public MoveActivityPermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.MoveActivity)
		{
		}
	}
}