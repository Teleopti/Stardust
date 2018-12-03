using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models
{
    public class AbsolutePositionViewModel : ActivityViewModel<ActivityNormalExtender>
    {
        public AbsolutePositionViewModel(IWorkShiftRuleSet workShiftRuleSet, ActivityNormalExtender extender) : base(workShiftRuleSet, extender)
        {
            APStartTime = ContainedEntity.ActivityPositionWithSegment.Period.StartTime;
            APEndTime = ContainedEntity.ActivityPositionWithSegment.Period.EndTime;
            APSegment = ContainedEntity.ActivityPositionWithSegment.Segment;
        }

        public ActivityExtenderType ActivityExtenderType { get; set; }

        public override object Count
        {
            get
            {
                return null;
            }
            set
            {
                // Nothing happens, there has to be a better way of not adding setter here
            }
        }

        public new Type TypeOfClass
        {
            get
            {
                Type type = null;
                if (ContainedEntity.GetType() == typeof(ActivityAbsoluteStartExtender))
                {
                    type = typeof(ActivityAbsoluteStartExtender);
                }
                else if (ContainedEntity.GetType() == typeof(ActivityRelativeStartExtender))
                {
                    type = typeof(ActivityRelativeStartExtender);
                }
                else if (ContainedEntity.GetType() == typeof(ActivityRelativeEndExtender))
                {
                    type = typeof(ActivityRelativeEndExtender);
                }
                return type;
            }
        }


        private bool validateTime()
        {
            bool status = true;

            if (ALMinTime.Equals(TimeSpan.Zero))
                status &= false;
            if (status && ALMaxTime.Equals(TimeSpan.Zero))
                status &= false;
            if (status && (ALMinTime > ALMaxTime))
                status &= false;
            if (ALSegment.Equals(TimeSpan.Zero))
                status &= false;

            if (status && APStartTime > APEndTime)
                status &= false;
            if (status && APSegment.Equals(TimeSpan.Zero))
                status &= false;

            if (status)
            {
                var alTimePeriodWithSegment = new TimePeriodWithSegment(new TimePeriod(ALMinTime,
                                                                                                         ALMaxTime),
                                                                                                         ALSegment);
                var apTimePeriodWithSegment = new TimePeriodWithSegment(new TimePeriod((TimeSpan)APStartTime,
                                                                                                         (TimeSpan)APEndTime),
                                                                                                         (TimeSpan)APSegment);

                ContainedEntity.ActivityLengthWithSegment = alTimePeriodWithSegment;
                ContainedEntity.ActivityPositionWithSegment = apTimePeriodWithSegment;
                ContainedEntity.ExtendWithActivity = CurrentActivity;
            }

            return status;
        }

        public override bool Validate()
        {
            return validateTime();
        }

    }
}
