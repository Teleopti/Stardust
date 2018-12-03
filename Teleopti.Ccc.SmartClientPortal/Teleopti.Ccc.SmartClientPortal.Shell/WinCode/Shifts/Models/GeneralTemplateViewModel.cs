using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models
{
    public class GeneralTemplateViewModel : BaseModel, IGeneralTemplateViewModel
    {
        private readonly ContractTimeLimiter _limiter;
        private string _accessibility;
        private readonly int _defaultSegment;
        private readonly IList<KeyValuePair<DefaultAccessibility, string>> _defaultAccessibilityList;

        public GeneralTemplateViewModel(IWorkShiftRuleSet workShiftRuleSet,
                                        int defaultSegmentValue) 
            : base(workShiftRuleSet)
        {
            _defaultSegment = defaultSegmentValue;

            _defaultAccessibilityList = LanguageResourceHelper.TranslateEnumToList<DefaultAccessibility>();

            IEnumerable<ContractTimeLimiter> limiters = WorkShiftRuleSet.LimiterCollection.OfType<ContractTimeLimiter>();

            if (!limiters.Any())
            {
                WorkShiftRuleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0),
                                                           new TimeSpan(8, 0, 0)),
                                                           TimeSpan.FromMinutes(_defaultSegment)));
            }
            _limiter = limiters.First();

            StartPeriodStartTime = WorkShiftRuleSet.TemplateGenerator.StartPeriod.Period.StartTime;
            StartPeriodEndTime = WorkShiftRuleSet.TemplateGenerator.StartPeriod.Period.EndTime;
            StartPeriodSegment = WorkShiftRuleSet.TemplateGenerator.StartPeriod.Segment;

            EndPeriodStartTime = WorkShiftRuleSet.TemplateGenerator.EndPeriod.Period.StartTime;
            EndPeriodEndTime = WorkShiftRuleSet.TemplateGenerator.EndPeriod.Period.EndTime;
            EndPeriodSegment = WorkShiftRuleSet.TemplateGenerator.EndPeriod.Segment;

            WorkingStartTime = _limiter.TimeLimit.StartTime;
            WorkingEndTime = _limiter.TimeLimit.EndTime;
            WorkingSegment = _limiter.LengthSegment;

            BaseActivity = WorkShiftRuleSet.TemplateGenerator.BaseActivity;
            
            _accessibility = (from p in _defaultAccessibilityList where p.Key == workShiftRuleSet.DefaultAccessibility select p.Value).First();
        }

        public string Accessibility
        {
            get
            {
                return _accessibility;
            }
            set
            {
                _accessibility = value;
                WorkShiftRuleSet.DefaultAccessibility = getAccessibilityFromText(value);
            }
        }

    	public bool OnlyForRestrictions
    	{
			get { return WorkShiftRuleSet.OnlyForRestrictions; }
			set { WorkShiftRuleSet.OnlyForRestrictions = value; }
    	}

        public IActivity BaseActivity { get; set; }

        public IShiftCategory Category
        {
            get
            {
                return WorkShiftRuleSet.TemplateGenerator.Category;
            }
            set
            {
                WorkShiftRuleSet.TemplateGenerator.Category = value;
            }
        }

        public TimeSpan StartPeriodStartTime { get; set; }

        public TimeSpan StartPeriodEndTime { get; set; }

        public TimeSpan StartPeriodSegment { get; set; }

        public TimeSpan EndPeriodStartTime { get; set; }

        public TimeSpan EndPeriodEndTime { get; set; }

        public TimeSpan EndPeriodSegment { get; set; }

        public TimeSpan WorkingSegment { get; set; }

        public TimeSpan WorkingStartTime { get; set; }

        public TimeSpan WorkingEndTime { get; set; }

        public override bool Validate()
        {
            bool status = true;

            if (!ValidateStartEndTimes())
                status = false;

            if (!ValidateWorkingTimes())
                status = false;

            if (status)
            {
                var startTimePeriod = new TimePeriodWithSegment(new TimePeriod(StartPeriodStartTime, StartPeriodEndTime), StartPeriodSegment);
                var endTimePeriod = new TimePeriodWithSegment(new TimePeriod(EndPeriodStartTime, EndPeriodEndTime), EndPeriodSegment);
                WorkShiftRuleSet.TemplateGenerator.StartPeriod = startTimePeriod;
                WorkShiftRuleSet.TemplateGenerator.EndPeriod = endTimePeriod;

                _limiter.TimeLimit = new TimePeriod(WorkingStartTime, WorkingEndTime);
                _limiter.LengthSegment = WorkingSegment;
            }
            WorkShiftRuleSet.TemplateGenerator.BaseActivity = BaseActivity;

            return status;
        }

        public bool ValidateEarlyStartTime()
        {
            bool status = true;

            if (StartPeriodStartTime > new TimeSpan(23, 59, 59))
                status = false;

            return status;
        }

        public bool ValidateLateStartTime()
        {
            bool status = true;

            if (StartPeriodEndTime > new TimeSpan(23, 59, 59))
                status = false;

            return status;
        }

        public bool ValidateEarlyEndTime()
        {
            bool status = true;

            if (EndPeriodStartTime.Equals((TimeSpan.Zero)))
                EndPeriodStartTime = new TimeSpan(1, EndPeriodStartTime.Hours, EndPeriodStartTime.Minutes, EndPeriodStartTime.Seconds);

            if (EndPeriodStartTime >= new TimeSpan(2, 0, 0, 0))
                status = false;
            
            return status;
        }

        public bool ValidateLateEndTime()
        {
            bool status = true;

            if (EndPeriodEndTime.Equals((TimeSpan.Zero)))
                EndPeriodEndTime = new TimeSpan(1, EndPeriodEndTime.Hours, EndPeriodEndTime.Minutes, EndPeriodEndTime.Seconds);

            if (EndPeriodEndTime >= new TimeSpan(2, 0, 0, 0))
                status = false;

            return status;
        }

        public bool ValidateStartTimes()
        {
            bool status = true;

            if (ValidateEarlyStartTime() == false)
                status = false;
            if (ValidateLateStartTime() == false)
                status = false;
            if (StartPeriodSegment == TimeSpan.Zero)
                status = false;
            if (StartPeriodEndTime < StartPeriodStartTime)
                status = false;

            return status;
        }

        public bool ValidateEndTimes()
        {
            bool status = true;

            if (ValidateEarlyEndTime() == false)
                status = false;
            if (ValidateLateEndTime() == false)
                status = false;
            if (EndPeriodSegment == TimeSpan.Zero)
                status = false;
            if (EndPeriodEndTime < EndPeriodStartTime)
                status = false;

            return status;
        }

        public bool ValidateStartEndTimes()
        {
            bool status = true;

            if (ValidateStartTimes() == false)
                status = false;
            if (ValidateEndTimes() == false)
                status = false;

            if (EndPeriodStartTime < StartPeriodStartTime)
                status = false;

            if (EndPeriodEndTime < StartPeriodEndTime)
                status = false;
            return status;
        }

        public bool ValidateWorkingTimes()
        {
            bool status = true;
            if (ValidateWorkingTimeLength() == false)
                status = false;

            if (WorkingStartTime > WorkingEndTime)
                status = false;

            return status;
        }

        public bool ValidateWorkingTimeLength()
        {
            bool status = true;

            if (WorkingStartTime.Equals(TimeSpan.Zero))
                status = false;

            if (WorkingEndTime > TimeSpan.FromHours(36))
                status = false;

            if (WorkingSegment.Equals(TimeSpan.Zero))
                status = false;

            if (WorkingSegment > TimeSpan.FromHours(36))
                status = false;
            return status;
        }


        private static DefaultAccessibility getAccessibilityFromText(string text)
        {
            DefaultAccessibility returnValue = DefaultAccessibility.Included;
            IList<KeyValuePair<DefaultAccessibility, string>> pair = LanguageResourceHelper.TranslateEnumToList<DefaultAccessibility>();
            foreach (KeyValuePair<DefaultAccessibility, string> value in pair)
            {
                if (string.Compare(value.Value, text, StringComparison.CurrentCulture) == 0)
                {
                    returnValue = value.Key;
                    break;
                }
            }
            return returnValue;
        }
    }
}
