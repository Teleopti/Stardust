using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IAlreadyAbsentSpecification : ISpecification<IAbsenceRequestAndSchedules>
	{}

	public class AlreadyAbsentSpecification : Specification<IAbsenceRequestAndSchedules>, IAlreadyAbsentSpecification
	{
		public override bool IsSatisfiedBy(IAbsenceRequestAndSchedules obj)
		{
			var scheduleDays = obj.SchedulingResultStateHolder.Schedules[obj.AbsenceRequest.Person]
				.ScheduledDayCollection(
					obj.AbsenceRequest.Period.ToDateOnlyPeriod(obj.AbsenceRequest.Person.PermissionInformation.DefaultTimeZone()));
			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var absenceLayers = projection.FilterLayers(obj.AbsenceRequest.Period).FilterLayers<IAbsence>();
				if (absenceLayers.Count()>0)
				{
					return true;
				}
			}

			return false;
		}
	}
}