using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Fairness
{
    public interface IValidateScheduleDays
    {
        bool Validate(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2);
    }

    public class ValidateScheduleDays : IValidateScheduleDays
    {
        public bool Validate(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
        {
            return true;
        }
    }
}