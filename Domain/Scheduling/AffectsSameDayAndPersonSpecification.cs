using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling
{

    /// <summary>
    /// Specification for verifying if a schedule affects the same date and person
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-06-11
    /// </remarks>
    public class ScheduleAffectsSameDayAndPerson : Specification<IScheduleDay>
    {
        private IScheduleDay _scheduleDay;


        public ScheduleAffectsSameDayAndPerson(IScheduleDay scheduleDay)
        {
            _scheduleDay = scheduleDay;
        }

        public override bool IsSatisfiedBy(IScheduleDay obj)
        {

            return 
                obj != null &&
                obj.Person == _scheduleDay.Person &&
                obj.DateOnlyAsPeriod.DateOnly.Equals(_scheduleDay.DateOnlyAsPeriod.DateOnly);
        }
    }
}
