using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models
{
    public class AutoPositionViewModel : ActivityViewModel<AutoPositionedActivityExtender>
    {
        private TimeSpan _aPSegment;

        public AutoPositionViewModel(IWorkShiftRuleSet workShiftRuleSet, 
                                     AutoPositionedActivityExtender activityExtender) 
            : base(workShiftRuleSet, activityExtender)
        {
            _aPSegment = activityExtender.AutoPositionIntervalSegment;
            SetAutoPosition(true);
        }

        public override object Count
        {
            get { return ContainedEntity.NumberOfLayers;}
            set
            {
                byte nValue;
                if (Byte.TryParse(value.ToString(), out nValue))
                    ContainedEntity.NumberOfLayers = nValue;
            }
        }

        public override TimeSpan? APStartTime
        {
            get { return null;}
        }

        public override TimeSpan? APEndTime
        {
            get { return null;}
        }

        public override TimeSpan APSegment
        {
            get { return _aPSegment;}
            set { _aPSegment = value;}
        }

        public override bool Validate()
        {
            bool status = true;

            if (ALMinTime.Equals(TimeSpan.Zero))
                status = false;
            if (status && ALMaxTime.Equals(TimeSpan.Zero))
                status = false;
            if (status && ALMaxTime < ALMinTime)
                status = false;
            if (ALSegment.Equals(TimeSpan.Zero))
                status = false;
            if (APSegment.Equals(TimeSpan.Zero))
                status = false;

            if (status)
            {
                var alTimePeriodWithSegment = new TimePeriodWithSegment(new TimePeriod(ALMinTime,ALMaxTime),
                                                                                                 ALSegment);

                ContainedEntity.ActivityLengthWithSegment = alTimePeriodWithSegment;
                ContainedEntity.AutoPositionIntervalSegment = APSegment;
                ContainedEntity.ExtendWithActivity = CurrentActivity;
            }

            return status;
        }

    }
}
