using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AlreadyAbsentValidator
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public AlreadyAbsentValidator(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public virtual bool Validate(IAbsenceRequest absenceRequest)
		{
			var scheduleDays = _schedulingResultStateHolder.Schedules[absenceRequest.Person]
				.ScheduledDayCollection(
					absenceRequest.Period.ToDateOnlyPeriod(absenceRequest.Person.PermissionInformation.DefaultTimeZone()));
			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var absenceLayers = projection.FilterLayers(absenceRequest.Period).FilterLayers<IAbsence>();
				if (absenceLayers.Count()>0)
				{
					return false;
				}
			}

			return true;
		}
	}
}