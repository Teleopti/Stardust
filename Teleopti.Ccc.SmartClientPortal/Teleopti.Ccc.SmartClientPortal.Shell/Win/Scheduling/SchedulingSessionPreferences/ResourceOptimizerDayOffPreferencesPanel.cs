using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class ResourceOptimizerDayOffPreferencesPanel : BaseUserControl, IDataExchange
    {

        #region Variables

		private IDaysOffPreferences _ruleSet;

        #endregion

        #region Constructor and initialize

        public ResourceOptimizerDayOffPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Initializes the specified rule set. Note: As a rule you call this method it before you start using the component.
        /// </summary>
        /// <param name="ruleSet">The rule set.</param>
        public void Initialize(IDaysOffPreferences ruleSet)
        {
            _ruleSet = ruleSet;
            ExchangeData(ExchangeDataOption.ServerToClient);
        }

        #endregion

        public event EventHandler<EventArgs> StatusChanged;

        public bool StatusIsOn()
        {
            if(!checkBoxUseFreeWeekends.Checked && !checkBoxUseFreeWeekEndDays.Checked && 
                !checkBoxUseDaysOffPerWeek.Checked && !checkBoxUseConsecutiveWorkDays.Checked && 
                !checkBoxUseConsecutiveDaysOff.Checked)
            {
                return false;
            }

            return true;
        }

        #region Interface

        #region IDataExchange Members

        public bool ValidateData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.ClientToServer)
                return validateRuleSet();

             return true;
            
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            if(direction == ExchangeDataOption.ServerToClient)
            {
                setFormData();
            }
            else
            {
                getFormData();
            }
        }

        #endregion

        #endregion

        #region Local

        private void onStatusChanged()
        {
            if(StatusChanged != null)
            {
                StatusChanged(this, new EventArgs());
            }
        }

        private bool validateRuleSet()
        {
            bool ret = true;
            if (!validateMinMax(checkBoxUseDaysOffPerWeek, numericUpDownDaysOffPerWeekMin, numericUpDownDaysOffPerWeekMax, UserTexts.Resources.MinDaysOffPerWeekMustBeLowerOrEqualToMax))
                ret = false;

            if (!validateMinMax(checkBoxUseConsecutiveDaysOff, numericUpDownConsDayOffMin, numericUpDownConsDayOffMax, UserTexts.Resources.ConsecutiveDaysOffPerWeekMustBeLowerOrEqualToMax))
                ret = false;

            if (!validateMinMax(checkBoxUseConsecutiveWorkDays, numericUpDownConsWorkDaysMin, numericUpDownConsWorkDaysMax, UserTexts.Resources.ConsecutiveWorkdaysPerWeekMustBeLowerOrEqualToMax))
                ret = false;

            if (!validateMinMax(checkBoxUseFreeWeekends, numericUpDownFreeWeekEndsMin, numericUpDownFreeWeekEndsMax, UserTexts.Resources.UseFreeWeekEnds))
                ret = false;

            if (!validateMinMax(checkBoxUseFreeWeekEndDays, numericUpDownFreeWeekEndDaysMin, numericUpDownFreeWeekEndDaysMax, UserTexts.Resources.UseFreeWeekEndDays))
                ret = false;

            return ret;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private bool validateMinMax(CheckBoxAdv checkBoxToUse, NumericUpDown numericUpDownMin, NumericUpDown numericUpDownMax, string message)
        {
            if (checkBoxToUse.Checked)
            {
                if (numericUpDownMax.Value < numericUpDownMin.Value)
                {
                    ViewBase.ShowInformationMessage(this, message, UserTexts.Resources.DayOffReoptimizerOptions);
                    
                    return false;
                }
            }

            return true;
        }

        private void getFormData()
        {
            _ruleSet.UseDaysOffPerWeek = checkBoxUseDaysOffPerWeek.Checked;
            if(_ruleSet.UseDaysOffPerWeek)
            {
                _ruleSet.DaysOffPerWeekValue = new MinMax<int>((int)numericUpDownDaysOffPerWeekMin.Value, (int)numericUpDownDaysOffPerWeekMax.Value);
            }
            _ruleSet.UseConsecutiveDaysOff = checkBoxUseConsecutiveDaysOff.Checked;
            if (_ruleSet.UseConsecutiveDaysOff)
            {
                _ruleSet.ConsecutiveDaysOffValue = new MinMax<int>((int)numericUpDownConsDayOffMin.Value, (int)numericUpDownConsDayOffMax.Value);
            }
            _ruleSet.UseConsecutiveWorkdays = checkBoxUseConsecutiveWorkDays.Checked;
            if (_ruleSet.UseConsecutiveWorkdays)
            {
                _ruleSet.ConsecutiveWorkdaysValue = new MinMax<int>((int)numericUpDownConsWorkDaysMin.Value, (int)numericUpDownConsWorkDaysMax.Value);
            }

            _ruleSet.UseFullWeekendsOff = checkBoxUseFreeWeekends.Checked;
            if (_ruleSet.UseFullWeekendsOff)
            {
                _ruleSet.FullWeekendsOffValue = new MinMax<int>((int)numericUpDownFreeWeekEndsMin.Value, (int)numericUpDownFreeWeekEndsMax.Value);
            }

            _ruleSet.UseWeekEndDaysOff = checkBoxUseFreeWeekEndDays.Checked;
            if (_ruleSet.UseWeekEndDaysOff)
            {
                _ruleSet.WeekEndDaysOffValue = new MinMax<int>((int)numericUpDownFreeWeekEndDaysMin.Value, (int)numericUpDownFreeWeekEndDaysMax.Value);
            }


            _ruleSet.KeepFreeWeekendDays = checkBoxKeepFreeWeekEndDays.Checked;
            _ruleSet.KeepFreeWeekends = checkBoxKeepFreeWeekEnds.Checked;

            _ruleSet.ConsiderWeekBefore = checkBoxConsiderWeekBefore.Checked;
            _ruleSet.ConsiderWeekAfter = checkBoxConsiderWeekAfter.Checked;
        }

        private void setFormData()
        {
            checkBoxUseDaysOffPerWeek.Checked =_ruleSet.UseDaysOffPerWeek;
            
            numericUpDownDaysOffPerWeekMin.Value = _ruleSet.DaysOffPerWeekValue.Minimum;
			numericUpDownDaysOffPerWeekMax.Value = _ruleSet.DaysOffPerWeekValue.Maximum;
            
            checkBoxUseConsecutiveDaysOff.Checked = _ruleSet.UseConsecutiveDaysOff;
            numericUpDownConsDayOffMin.Value = _ruleSet.ConsecutiveDaysOffValue.Minimum;
			numericUpDownConsDayOffMax.Value = _ruleSet.ConsecutiveDaysOffValue.Maximum;

            checkBoxUseConsecutiveWorkDays.Checked = _ruleSet.UseConsecutiveWorkdays;
            numericUpDownConsWorkDaysMin.Value = _ruleSet.ConsecutiveWorkdaysValue.Minimum;
			numericUpDownConsWorkDaysMax.Value = _ruleSet.ConsecutiveWorkdaysValue.Maximum;

            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxKeepFreeWeekEndDays.Checked = _ruleSet.KeepFreeWeekendDays;
            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxKeepFreeWeekEnds.Checked = _ruleSet.KeepFreeWeekends;
            checkBoxConsiderWeekBefore.Checked = _ruleSet.ConsiderWeekBefore;
            checkBoxConsiderWeekAfter.Checked = _ruleSet.ConsiderWeekAfter;

            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxUseFreeWeekends.Checked = _ruleSet.UseFullWeekendsOff;
            numericUpDownFreeWeekEndsMin.Value = _ruleSet.FullWeekendsOffValue.Minimum;
			numericUpDownFreeWeekEndsMax.Value = _ruleSet.FullWeekendsOffValue.Maximum;

            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxUseFreeWeekEndDays.Checked = _ruleSet.UseWeekEndDaysOff;
            numericUpDownFreeWeekEndDaysMin.Value = _ruleSet.WeekEndDaysOffValue.Minimum;
			numericUpDownFreeWeekEndDaysMax.Value = _ruleSet.WeekEndDaysOffValue.Maximum;

            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            toggleFreeWeekEnds(!_ruleSet.KeepFreeWeekends);
            toggleFreeWeekEndDays(!_ruleSet.KeepFreeWeekendDays);

        }

        #endregion

        private void checkBoxKeepFreeWeekEnds_CheckedChanged(object sender, System.EventArgs e)
        {
            toggleFreeWeekEnds(!((CheckBox)sender).Checked);
            onStatusChanged();
        }

        private void checkBoxKeepFreeWeekEndDays_CheckedChanged(object sender, System.EventArgs e)
        {
            toggleFreeWeekEndDays(!((CheckBox)sender).Checked);
            onStatusChanged();
        }

        private void toggleFreeWeekEnds(bool value)
        {
            if (value == false)
                checkBoxUseFreeWeekends.Checked = false;

            checkBoxUseFreeWeekends.Enabled = value;
            numericUpDownFreeWeekEndsMin.Enabled = value;
            numericUpDownFreeWeekEndsMax.Enabled = value;
        }

        private void toggleFreeWeekEndDays(bool value)
        {
            if (value == false)
                checkBoxUseFreeWeekEndDays.Checked = false;

            checkBoxUseFreeWeekEndDays.Enabled = value;
            numericUpDownFreeWeekEndDaysMin.Enabled = value;
            numericUpDownFreeWeekEndDaysMax.Enabled = value;
        }

        private void checkbox_checkedChanged(object sender, EventArgs e)
        {
            onStatusChanged();
        }
    }
}