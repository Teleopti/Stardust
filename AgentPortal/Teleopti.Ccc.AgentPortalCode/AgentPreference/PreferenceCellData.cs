using System;
using System.Drawing;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IPreferenceCellData
    {
        DateTime TheDate { get; set; }
        bool Enabled { get; set; }
        bool Legal { get; set; }
        TimeSpan WeeklyMax { get; set; }
        TimeSpan PeriodTarget { get; set; }
        TimeSpan BalancedPeriodTarget { get; set; }
        TimePeriod BalancedPeriodTargetWithTolerance { get; set; }
        TimeSpan BalanceIn { get; set; }
        TimeSpan Extra { get; set; }
        TimeSpan BalanceOut { get; set; }
        double Seasonality { get; set; }
        int PeriodDayOffsTarget { get; set; }
        int PeriodDayOffs { get; set; }
        bool IsInsidePeriod { get; set; }
        Color DisplayColor { get; set; }
        string DisplayName { get; set; }
        string DisplayShortName { get; set; }
        bool HasShift { get; set; }
        bool HasDayOff { get; set; }
        bool HasAbsence { get; set; }
        bool HasPersonalAssignmentOnly { get; set; }
        string TipText { get; set; }
        int MaxMustHave { get; set; }
        Preference Preference { get; set; }
        EffectiveRestriction EffectiveRestriction { get; set; }
        bool IsWorkday { get; set; }
        bool ViolatesNightlyRest { get; set; }
    }

    public class PreferenceCellData : IPreferenceCellData
    {
        private Preference _preference;
        private EffectiveRestriction _effectiveRestriction;

        public PreferenceCellData()
        {
            Enabled = true;
        }

        public DateTime TheDate { get; set; }

        public bool Enabled { get; set; }
        public bool Legal { get; set; }
        public TimeSpan WeeklyMax { get; set; }
        public TimeSpan PeriodTarget { get; set; }
        public TimeSpan BalancedPeriodTarget { get; set; }
        public TimePeriod BalancedPeriodTargetWithTolerance { get; set; }
        public TimeSpan BalanceIn { get; set; }
        public TimeSpan Extra { get; set; }
        public TimeSpan BalanceOut { get; set; }
        public double Seasonality { get; set; }

        public int PeriodDayOffsTarget { get; set; }
        public int PeriodDayOffs { get; set; }
        public bool IsInsidePeriod { get; set; }
        public Color DisplayColor { get; set; }
        public string DisplayName { get; set; }
        public string DisplayShortName { get; set; }
        public bool HasShift { get; set; }
        public bool HasDayOff { get; set; }
        public bool HasAbsence { get; set; }
        public bool HasPersonalAssignmentOnly { get; set; }
        public string TipText { get; set; }
        public int MaxMustHave { get; set; }
        public Preference Preference
        {
            get { return _preference; }
            set
            {
                if(Enabled)
                {
                    EffectiveRestriction = null;
                    _preference = value;
                }
            }
        }

        public EffectiveRestriction EffectiveRestriction
        {
            get { return _effectiveRestriction; }
            set { _effectiveRestriction = value; }
        }

        public bool IsWorkday { get; set; }

        public bool ViolatesNightlyRest { get; set; }
    }
}