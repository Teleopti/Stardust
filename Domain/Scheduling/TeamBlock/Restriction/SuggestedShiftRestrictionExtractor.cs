using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public interface ISuggestedShiftRestrictionExtractor
    {
        IEffectiveRestriction Extract(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions);
        IEffectiveRestriction ExtractForOneBlock(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions);
        IEffectiveRestriction ExtractForOneTeam(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions);
    }

    public class SuggestedShiftRestrictionExtractor : ISuggestedShiftRestrictionExtractor
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"),
         SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IEffectiveRestriction Extract(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions)
        {
            var startTimeLimitation = new StartTimeLimitation();
            var endTimeLimitation = new EndTimeLimitation();
            var workTimeLimitation = new WorkTimeLimitation();
            IEditableShift commonMainShift = null;
            IShiftCategory shiftCategory = null;
            if ((schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameStartTime)
                || (schedulingOptions.UseGroupScheduling && schedulingOptions.UseGroupSchedulingCommonStart))
            {
                TimeSpan startTime = shift.WorkShiftStartTime;
                startTimeLimitation = new StartTimeLimitation(startTime, startTime);
            }

            if ((schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameEndTime)
                || (schedulingOptions.UseGroupScheduling && schedulingOptions.UseGroupSchedulingCommonEnd))
            {
                TimeSpan endTime = shift.WorkShiftEndTime;
                endTimeLimitation = new EndTimeLimitation(endTime, endTime);
            }

            if (schedulingOptions.UseTeamBlockSameShift)
            {
                commonMainShift = shift.TheMainShift;
            }

            if ((schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameShiftCategory) ||
                (schedulingOptions.UseGroupScheduling && schedulingOptions.UseGroupSchedulingCommonCategory))
            {
                shiftCategory = shift.TheWorkShift.ShiftCategory;
            }

            var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                       workTimeLimitation, null, null, null,
                                                       new List<IActivityRestriction>())
                {
                    CommonMainShift = commonMainShift,
                    ShiftCategory = shiftCategory
                };

            return restriction;
        }

        public IEffectiveRestriction ExtractForOneBlock(IShiftProjectionCache shift,
                                                        ISchedulingOptions schedulingOptions)
        {
            var startTimeLimitation = new StartTimeLimitation();
            var endTimeLimitation = new EndTimeLimitation();
            var workTimeLimitation = new WorkTimeLimitation();
            IEditableShift commonMainShift = null;
            IShiftCategory shiftCategory = null;

            if (schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameStartTime)
            {
                TimeSpan startTime = shift.WorkShiftStartTime;
                startTimeLimitation = new StartTimeLimitation(startTime, startTime);
            }

            if (schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameEndTime)
            {
                TimeSpan endTime = shift.WorkShiftEndTime;
                endTimeLimitation = new EndTimeLimitation(endTime, endTime);
            }

            if (schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameShift)
            {
                commonMainShift = shift.TheMainShift;
            }

            if (schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameShiftCategory)
            {
                shiftCategory = shift.TheWorkShift.ShiftCategory;
            }

            var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                       workTimeLimitation, null, null, null,
                                                       new List<IActivityRestriction>())
                {
                    CommonMainShift = commonMainShift,
                    ShiftCategory = shiftCategory
                };

            return restriction;
        }

        public IEffectiveRestriction ExtractForOneTeam(IShiftProjectionCache shift, ISchedulingOptions schedulingOptions)
        {
            var startTimeLimitation = new StartTimeLimitation();
            var endTimeLimitation = new EndTimeLimitation();
            var workTimeLimitation = new WorkTimeLimitation();
            IShiftCategory shiftCategory = null;
            if (schedulingOptions.UseGroupScheduling && schedulingOptions.UseGroupSchedulingCommonStart)
            {
                TimeSpan startTime = shift.WorkShiftStartTime;
                startTimeLimitation = new StartTimeLimitation(startTime, startTime);
            }

            if (schedulingOptions.UseGroupScheduling && schedulingOptions.UseGroupSchedulingCommonEnd)
            {
                TimeSpan endTime = shift.WorkShiftEndTime;
                endTimeLimitation = new EndTimeLimitation(endTime, endTime);
            }

            if (schedulingOptions.UseGroupScheduling && schedulingOptions.UseGroupSchedulingCommonCategory)
            {
                shiftCategory = shift.TheWorkShift.ShiftCategory;
            }

            var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                       workTimeLimitation, null, null, null,
                                                       new List<IActivityRestriction>())
                {
                    ShiftCategory = shiftCategory
                };

            return restriction;
        }
    }
}