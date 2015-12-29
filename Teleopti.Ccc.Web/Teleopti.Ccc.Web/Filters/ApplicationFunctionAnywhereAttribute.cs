using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Web.Filters
{
	[CLSCompliant(false)]
	public sealed class AddFullDayAbsencePermissionAttribute : ApplicationFunctionApiAttribute
	{
		public AddFullDayAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence)
		{
		}
	}

	[CLSCompliant(false)]
	public sealed class AddIntradayAbsencePermissionAttribute : ApplicationFunctionApiAttribute
	{
		public AddIntradayAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence)
		{
		}
	}

	[CLSCompliant(false)]
	public sealed class RemoveAbsencePermissionAttribute : ApplicationFunctionApiAttribute
	{
		public RemoveAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.RemoveAbsence)
		{
		}
	}

	[CLSCompliant(false)]
	public sealed class AddActivityPermissionAttribute : ApplicationFunctionApiAttribute
	{
		public AddActivityPermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddActivity)
		{
		}
	}

	[CLSCompliant(false)]
	public sealed class MoveActivityPermissionAttribute : ApplicationFunctionApiAttribute
	{
		public MoveActivityPermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.MoveActivity)
		{
		}
	}
}