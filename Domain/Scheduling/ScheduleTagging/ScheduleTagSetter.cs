﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public class ScheduleTagSetter : IScheduleTagSetter
    {
        private IScheduleTag _tag;

        public ScheduleTagSetter(IScheduleTag tag)
        {
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
            DateOnly dateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(scheduleDay.Period.StartDateTime, scheduleDay.Person.PermissionInformation.DefaultTimeZone()));
            IAgentDayScheduleTag agentDayScheduleTag = new AgentDayScheduleTag(scheduleDay.Person, dateOnly, scheduleDay.Scenario, _tag);
            scheduleDay.Clear<IAgentDayScheduleTag>();
            if (_tag is NullScheduleTag)
                return;

            scheduleDay.Add(agentDayScheduleTag);
        }
    }
}