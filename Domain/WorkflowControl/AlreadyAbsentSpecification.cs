using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AlreadyAbsentSpecification : Specification<IAbsenceRequest>
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public AlreadyAbsentSpecification(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public override bool IsSatisfiedBy(IAbsenceRequest obj)
		{
			var scheduleDays = _schedulingResultStateHolder.Schedules[obj.Person]
				.ScheduledDayCollection(
					obj.Period.ToDateOnlyPeriod(obj.Person.PermissionInformation.DefaultTimeZone()));
			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var absenceLayers = projection.FilterLayers(obj.Period).FilterLayers<IAbsence>();
				if (absenceLayers.Count()>0)
				{
					return true;
				}
			}

			return false;
		}
	}
}