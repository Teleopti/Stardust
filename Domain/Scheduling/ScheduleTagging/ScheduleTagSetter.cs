using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
	public class NoScheduleTagSetter : IScheduleTagSetter
	{
		public void SetTagOnScheduleDays(ScheduleModifier modifier, IEnumerable<IScheduleDay> scheduleParts)
		{
		}

		public void ChangeTagToSet(IScheduleTag tag)
		{
		}
	}

	public class ScheduleTagSetter : IScheduleTagSetter
    {
        private IScheduleTag _tag;

        public ScheduleTagSetter(IScheduleTag tag)
        {
        	if (tag == null) 
				throw new ArgumentNullException(nameof(tag));
        	_tag = tag;
        }

    	public void SetTagOnScheduleDays(ScheduleModifier modifier, IEnumerable<IScheduleDay> scheduleParts)
        {
            switch(modifier)
            {
                case ScheduleModifier.Scheduler:
                case ScheduleModifier.AutomaticScheduling:
                case ScheduleModifier.Request:
                    addToAll(scheduleParts);
                    break;
            }
        }

        public void ChangeTagToSet(IScheduleTag tag)
        {
        	if (tag == null) 
				throw new ArgumentNullException(nameof(tag));
        	_tag = tag;
        }

    	private void addToAll(IEnumerable<IScheduleDay> scheduleParts)
        {
            if (_tag is KeepOriginalScheduleTag) 
                return;

            foreach(var scheduleDay in scheduleParts)
            {
                createAndAddScheduleTag(scheduleDay);
            }
        }

        private void createAndAddScheduleTag(IScheduleDay scheduleDay)
        {
            DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
            IAgentDayScheduleTag agentDayScheduleTag = new AgentDayScheduleTag(scheduleDay.Person, dateOnly, scheduleDay.Scenario, _tag);
            scheduleDay.Clear<IAgentDayScheduleTag>();
            if (_tag is NullScheduleTag)
                return;

            scheduleDay.Add(agentDayScheduleTag);
        }
    }
}