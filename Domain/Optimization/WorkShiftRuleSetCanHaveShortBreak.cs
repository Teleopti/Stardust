using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class WorkShiftRuleSetCanHaveShortBreak : IWorkShiftRuleSetCanHaveShortBreak
    {
        private readonly ITimePeriodCanHaveShortBreak _timePeriodCanHaveShortBreak;
        private readonly ISkillExtractor _skillExtractor;

        public WorkShiftRuleSetCanHaveShortBreak(ITimePeriodCanHaveShortBreak timePeriodCanHaveShortBreak, ISkillExtractor skillExtractor)
        {
            _timePeriodCanHaveShortBreak = timePeriodCanHaveShortBreak;
            _skillExtractor = skillExtractor;
        }

        public bool CanHaveShortBreak(IWorkShiftRuleSet workShiftRuleSet)
        {
            if(CheckWorkShiftTemplateForShortBreak(workShiftRuleSet.TemplateGenerator))
                return true;

            if (CheckWorkShiftExtendersForShortBreak(workShiftRuleSet.ExtenderCollection)) 
                return true;
            
            return false;
        }

        private bool CheckWorkShiftTemplateForShortBreak(IWorkShiftTemplateGenerator workShiftTemplateGenerator)
        {
            TimePeriodWithSegment startTimePeriodWithSegment = workShiftTemplateGenerator.StartPeriod;
            if (_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, startTimePeriodWithSegment))
                return true;
            TimePeriodWithSegment endTimePeriodWithSegment = workShiftTemplateGenerator.EndPeriod;
            if (_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, endTimePeriodWithSegment))
                return true;
            return false;
        }

        private bool CheckWorkShiftExtendersForShortBreak(IEnumerable<IWorkShiftExtender> workShiftExtenders)
        {
            foreach (IWorkShiftExtender workShiftExtender in workShiftExtenders)
            {
                if (workShiftExtender is IAutoPositionedActivityExtender)
                {
                    if (CheckActivityExtender((IAutoPositionedActivityExtender)workShiftExtender))
                        return true;
                }
                else if(workShiftExtender is IActivityNormalExtender)
                {
                    if (CheckActivityExtender((IActivityNormalExtender)workShiftExtender))
                        return true;
                }
                else
                    throw new NotImplementedException("This type of parameter is unknown therefore not handled here."); 
            }
            return false;
        }

        private bool CheckActivityExtender(IAutoPositionedActivityExtender activityExtender)
        {
            TimePeriodWithSegment timePeriodWithSegment = activityExtender.ActivityLengthWithSegment;
            if (_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, timePeriodWithSegment))
                return true;
            if (_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor,
                new List<TimeSpan> { activityExtender.AutoPositionIntervalSegment, activityExtender.StartSegment }))
                return true;
            return false;
        }

        private bool CheckActivityExtender(IActivityNormalExtender activityExtender)
        {
            TimePeriodWithSegment timePeriodWithSegment = activityExtender.ActivityLengthWithSegment;
            if (_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, timePeriodWithSegment))
                return true;
            timePeriodWithSegment = activityExtender.ActivityPositionWithSegment;
            if (_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, timePeriodWithSegment))
                return true;
            return false;
        }
    }
}