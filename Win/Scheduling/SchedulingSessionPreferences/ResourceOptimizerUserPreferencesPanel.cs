using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class ResourceOptimizerUserPreferencesPanel : BaseUserControl, IDataExchange
    {
        public ResourceOptimizerUserPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Initializes the reoptimizer preferences data. Note: As a rule you call this method it before you start using the component.
        /// </summary>
        /// <param name="optimizerAdvancedPreferences">The advanced optimizer preferences.</param>
        public void Initialize(IOptimizerAdvancedPreferences optimizerAdvancedPreferences)
        {
            OptimizerAdvancedPreferences = optimizerAdvancedPreferences;
            ExchangeData(ExchangeDataOption.ServerToClient);
        }

        /// <summary>
        /// Gets or sets the re optimizer preferences.
        /// </summary>
        /// <value>The re optimizer preferences.</value>
        public IOptimizerAdvancedPreferences OptimizerAdvancedPreferences { get; set; }

        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            if(direction == ExchangeDataOption.ServerToClient)
            {
                numericUpDownMoveMaxDayOffs.Value = (decimal)OptimizerAdvancedPreferences.MaximumMovableDayOffPercentagePerPerson * 100;
                numericUpDownMoveMaxShifts.Value = (decimal)OptimizerAdvancedPreferences.MaximumMovableWorkShiftPercentagePerPerson * 100;
                checkBoxDaysOff.Checked = OptimizerAdvancedPreferences.AllowDayOffOptimization;
                checkBoxTimeBeetweenDays.Checked = OptimizerAdvancedPreferences.AllowWorkShiftOptimization;
                checkBoxShiftsWithinDay.Checked = OptimizerAdvancedPreferences.AllowIntradayOptimization;
                checkBoxExtendReduceTime.Checked = OptimizerAdvancedPreferences.AllowExtendReduceTimeOptimization;
                checkBoxExtendReduceDaysoff.Checked = OptimizerAdvancedPreferences.AllowExtendReduceDaysOffOptimization;
            }
            else
            {
                OptimizerAdvancedPreferences.MaximumMovableDayOffPercentagePerPerson = (double)numericUpDownMoveMaxDayOffs.Value / 100;
                OptimizerAdvancedPreferences.MaximumMovableWorkShiftPercentagePerPerson = (double)numericUpDownMoveMaxShifts.Value / 100;
                OptimizerAdvancedPreferences.AllowDayOffOptimization =  checkBoxDaysOff.Checked;
                OptimizerAdvancedPreferences.AllowWorkShiftOptimization = checkBoxTimeBeetweenDays.Checked;
                OptimizerAdvancedPreferences.AllowIntradayOptimization = checkBoxShiftsWithinDay.Checked;
                OptimizerAdvancedPreferences.AllowExtendReduceTimeOptimization = checkBoxExtendReduceTime.Checked;
                OptimizerAdvancedPreferences.AllowExtendReduceDaysOffOptimization = checkBoxExtendReduceDaysoff.Checked;
            }
        }
    }
}
