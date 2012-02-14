using System;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class ResourceOptimizerDayOffPreferencesPanel : BaseUserControl, IDataExchange
    {

        #region Variables

        private IDayOffPlannerRules _ruleSet;

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
        public void Initialize(IDayOffPlannerRules ruleSet)
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

        public IDayOffPlannerRules RuleSet
        {
            get { return _ruleSet; }
        }

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


        public bool KeepFreeWeekendsVisible
        {
            get { return checkBoxKeepFreeWeekEnds.Visible; }
            set { checkBoxKeepFreeWeekEnds.Visible = value; }
        }   

        public bool KeepFreeWeekendDaysVisible
        {
            get { return checkBoxKeepFreeWeekEndDays.Visible; }
            set { checkBoxKeepFreeWeekEndDays.Visible = value; }
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private bool validateMinMax(CheckBox checkBoxToUse, NumericUpDown numericUpDownMin, NumericUpDown numericUpDownMax, string message)
        {
            if (checkBoxToUse.Checked)
            {
                if (numericUpDownMax.Value < numericUpDownMin.Value)
                {
                    MessageBox.Show(this, string.Concat(message, " "), UserTexts.Resources.DayOffReoptimizerOptions,
                                                                MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                                                (RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
                    
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
                _ruleSet.DaysOffPerWeek = new MinMax<int>((int)numericUpDownDaysOffPerWeekMin.Value, (int)numericUpDownDaysOffPerWeekMax.Value);
            }
            _ruleSet.UseConsecutiveDaysOff = checkBoxUseConsecutiveDaysOff.Checked;
            if (_ruleSet.UseConsecutiveDaysOff)
            {
                _ruleSet.ConsecutiveDaysOff = new MinMax<int>((int)numericUpDownConsDayOffMin.Value, (int)numericUpDownConsDayOffMax.Value);
            }
            _ruleSet.UseConsecutiveWorkdays = checkBoxUseConsecutiveWorkDays.Checked;
            if (_ruleSet.UseConsecutiveWorkdays)
            {
                _ruleSet.ConsecutiveWorkdays = new MinMax<int>((int)numericUpDownConsWorkDaysMin.Value, (int)numericUpDownConsWorkDaysMax.Value);
            }

            _ruleSet.UseFreeWeekends = checkBoxUseFreeWeekends.Checked;
            if (_ruleSet.UseFreeWeekends)
            {
                _ruleSet.FreeWeekends = new MinMax<int>((int)numericUpDownFreeWeekEndsMin.Value, (int)numericUpDownFreeWeekEndsMax.Value);
            }

            _ruleSet.UseFreeWeekendDays = checkBoxUseFreeWeekEndDays.Checked;
            if (_ruleSet.UseFreeWeekendDays)
            {
                _ruleSet.FreeWeekendDays = new MinMax<int>((int)numericUpDownFreeWeekEndDaysMin.Value, (int)numericUpDownFreeWeekEndDaysMax.Value);
            }


            _ruleSet.KeepFreeWeekendDays = checkBoxKeepFreeWeekEndDays.Checked;
            _ruleSet.KeepFreeWeekends = checkBoxKeepFreeWeekEnds.Checked;

            _ruleSet.UsePreWeek = checkBoxConsiderWeekBefore.Checked;
            _ruleSet.UsePostWeek = checkBoxConsiderWeekAfter.Checked;
        }

        private void setFormData()
        {
            checkBoxUseDaysOffPerWeek.Checked =_ruleSet.UseDaysOffPerWeek;
            
            numericUpDownDaysOffPerWeekMin.Value = _ruleSet.DaysOffPerWeek.Minimum;
            numericUpDownDaysOffPerWeekMax.Value = _ruleSet.DaysOffPerWeek.Maximum;
            
            checkBoxUseConsecutiveDaysOff.Checked = _ruleSet.UseConsecutiveDaysOff;
            numericUpDownConsDayOffMin.Value = _ruleSet.ConsecutiveDaysOff.Minimum;
            numericUpDownConsDayOffMax.Value = _ruleSet.ConsecutiveDaysOff.Maximum;

            checkBoxUseConsecutiveWorkDays.Checked = _ruleSet.UseConsecutiveWorkdays;
            numericUpDownConsWorkDaysMin.Value = _ruleSet.ConsecutiveWorkdays.Minimum;
            numericUpDownConsWorkDaysMax.Value = _ruleSet.ConsecutiveWorkdays.Maximum;

            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxKeepFreeWeekEndDays.Checked = _ruleSet.KeepFreeWeekendDays;
            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxKeepFreeWeekEnds.Checked = _ruleSet.KeepFreeWeekends;
            checkBoxConsiderWeekBefore.Checked = _ruleSet.UsePreWeek;
            checkBoxConsiderWeekAfter.Checked = _ruleSet.UsePostWeek;

            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxUseFreeWeekends.Checked = _ruleSet.UseFreeWeekends;
            numericUpDownFreeWeekEndsMin.Value = _ruleSet.FreeWeekends.Minimum;
            numericUpDownFreeWeekEndsMax.Value = _ruleSet.FreeWeekends.Maximum;

            //default to unchecked for now, need to keep one state for scheduling and one for optimization
            checkBoxUseFreeWeekEndDays.Checked = _ruleSet.UseFreeWeekendDays;
            numericUpDownFreeWeekEndDaysMin.Value = _ruleSet.FreeWeekendDays.Minimum;
            numericUpDownFreeWeekEndDaysMax.Value = _ruleSet.FreeWeekendDays.Maximum;

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