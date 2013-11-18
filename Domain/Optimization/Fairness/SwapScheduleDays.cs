using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Fairness
{
    public interface ISwapScheduleDays
    {
        bool Swap(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2);
    }

    public class SwapScheduleDays : ISwapScheduleDays
    {
        public bool Swap(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
        {
            return true;
        }
    }
}