using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
 
    public class OptimizerAdvancedPreferences : IOptimizerAdvancedPreferences
    {
        #region Variables

        private const bool defaultAllowMoveTime = true;
        private const bool defaultAllowDayOffOptimization = true;
        private const bool defaultAllowIntradayOptimization = true;
        private const bool defaultAllowExtendReduceTimeOptimization = true;
        private const bool defaultAllowExtendReduceDaysoffOptimization = true;
        private const double defaultMaximumMoveDayOffPercentagePerPerson = 1;
        private const double defaultMaximumMoveShiftPercentagePerPerson = 1;
        private const bool defaultUseMaximumStandardDeviation = false;
        private const bool defaultUseTewakedValues = true;
        private const bool defaultUseStandardDeviationCalculation = true;


        private bool _allowMoveTime = defaultAllowMoveTime;
        private bool _allowDayOffOptimization = defaultAllowDayOffOptimization;
        private bool _allowIntradayOptimization = defaultAllowIntradayOptimization;
        private bool _allowExtendReduceTimeOptimization = defaultAllowExtendReduceTimeOptimization;
        private bool _allowExtendReduceDaysOffOptimization = defaultAllowExtendReduceDaysoffOptimization;
        private double _maximumMoveDayOffPercentagePerPerson = defaultMaximumMoveDayOffPercentagePerPerson;
        private double _maximumMoveShiftPercentagePerPerson = defaultMaximumMoveShiftPercentagePerPerson;
        private bool _useMaximumStandardDeviation = defaultUseMaximumStandardDeviation;
        private bool _useTweakedValues = defaultUseTewakedValues;
        private bool _useStandardDeviationCalculation = defaultUseStandardDeviationCalculation;
        private bool _useTeleoptiCalculation;

        #endregion

        #region Interface

        #region IOptimizerAdvancedPreferences members

        [DefaultValue(defaultAllowMoveTime)]
        public bool AllowWorkShiftOptimization
        {
            get { return _allowMoveTime; }
            set { _allowMoveTime = value; }
        }

        [DefaultValue(defaultAllowExtendReduceDaysoffOptimization)]
        public bool AllowExtendReduceDaysOffOptimization
        {
            get { return _allowExtendReduceDaysOffOptimization; }
            set { _allowExtendReduceDaysOffOptimization = value; }
        }

        [DefaultValue(defaultAllowExtendReduceTimeOptimization)]
        public bool AllowExtendReduceTimeOptimization
        {
            get { return _allowExtendReduceTimeOptimization; }
            set { _allowExtendReduceTimeOptimization = value; }
        }

        [DefaultValue(defaultAllowDayOffOptimization)]
        public bool AllowDayOffOptimization
        {
            get { return _allowDayOffOptimization; }
            set { _allowDayOffOptimization = value; }
        }

        [DefaultValue(defaultAllowIntradayOptimization)]
        public bool AllowIntradayOptimization
        {
            get { return _allowIntradayOptimization; }
            set { _allowIntradayOptimization = value; }
        }

        [DefaultValue(defaultMaximumMoveDayOffPercentagePerPerson)]
        public double MaximumMovableDayOffPercentagePerPerson
        {
            get { return _maximumMoveDayOffPercentagePerPerson; }
            set { _maximumMoveDayOffPercentagePerPerson = value; }
        }

        [DefaultValue(defaultMaximumMoveShiftPercentagePerPerson)]
        public double MaximumMovableWorkShiftPercentagePerPerson
        {
            get { return _maximumMoveShiftPercentagePerPerson; }
            set { _maximumMoveShiftPercentagePerPerson = value; }
        }

        [DefaultValue(defaultUseMaximumStandardDeviation)]
        public bool ConsiderMaximumIntraIntervalStandardDeviation
        {
            get { return _useMaximumStandardDeviation; }
            set { _useMaximumStandardDeviation = value; }
        }

        [DefaultValue(defaultUseTewakedValues)]
        public bool UseTweakedValues
        {
            get { return _useTweakedValues; }
            set { _useTweakedValues = value; }
        }

        [DefaultValue(defaultUseStandardDeviationCalculation)]
        public bool UseStandardDeviationCalculation
        {
            get { return _useStandardDeviationCalculation; }
            set { _useStandardDeviationCalculation = value; }
        }

        [DefaultValue(false)]
        public bool UseTeleoptiCalculation
        {
            get { return _useTeleoptiCalculation; }
            set { _useTeleoptiCalculation = value; }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        #endregion
    }
}
