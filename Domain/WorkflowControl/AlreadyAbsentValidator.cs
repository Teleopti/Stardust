using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AlreadyAbsentValidator : IAlreadyAbsentValidator
	{
		public bool Validate(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange)
		{
			var period = absenceRequest.Period;
			var scheduleDays = scheduleRange.ScheduledDayCollection(
				period.ToDateOnlyPeriod(absenceRequest.Person.PermissionInformation.DefaultTimeZone()).Inflate(1));
			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var absenceLayers = projection.FilterLayers(period).FilterLayers<IAbsence>();
				if (absenceLayers.Any()) return true;
			}
			return false;
		}
	}
}