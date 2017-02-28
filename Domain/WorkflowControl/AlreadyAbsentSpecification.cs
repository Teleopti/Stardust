using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IAlreadyAbsentSpecification : ISpecification<IAbsenceRequestAndSchedules>
	{}

	public class AlreadyAbsentSpecification : Specification<IAbsenceRequestAndSchedules>, IAlreadyAbsentSpecification
	{
		private readonly IAlreadyAbsentValidator _alreadyAbsentValidator;

		public AlreadyAbsentSpecification(IAlreadyAbsentValidator alreadyAbsentValidator)
		{
			_alreadyAbsentValidator = alreadyAbsentValidator;
		}

		public override bool IsSatisfiedBy(IAbsenceRequestAndSchedules obj)
		{
			var scheduleRange = obj.SchedulingResultStateHolder.Schedules[obj.AbsenceRequest.Person];
			return _alreadyAbsentValidator.Validate(obj.AbsenceRequest, scheduleRange);
		}
	}
}