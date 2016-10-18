using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AlreadyAbsentValidator : IAlreadyAbsentValidator
	{
		public bool Validate(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange)
		{
			var period = absenceRequest.Period;
			var scheduleDays = scheduleRange.ScheduledDayCollection(
				period.ToDateOnlyPeriod(absenceRequest.Person.PermissionInformation.DefaultTimeZone()));
			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var absenceLayers = projection.FilterLayers(period).FilterLayers<IAbsence>();
				return absenceLayers.Any();
			}
			return false;
		}
	}
}