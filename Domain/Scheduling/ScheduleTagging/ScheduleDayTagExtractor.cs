using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public interface IScheduleDayTagExtractor
    {
        IList<IScheduleDay> All();
        IList<IScheduleDay> Tag(IScheduleTag scheduleTag);
    }

    public class ScheduleDayTagExtractor : IScheduleDayTagExtractor
    {
        private readonly IList<IScheduleDay> _scheduleDays;
        
        public ScheduleDayTagExtractor(IList<IScheduleDay> scheduleDays)
        {
            _scheduleDays = scheduleDays;
        }

        public IList<IScheduleDay> All()
        {
            return _scheduleDays.Where(scheduleDay => scheduleDay.ScheduleTag() != NullScheduleTag.Instance).ToList();
        }

        public IList<IScheduleDay> Tag(IScheduleTag scheduleTag)
        {
            return _scheduleDays.Where(scheduleDay => scheduleDay.ScheduleTag().Equals(scheduleTag)).ToList();
        }
    }
}
