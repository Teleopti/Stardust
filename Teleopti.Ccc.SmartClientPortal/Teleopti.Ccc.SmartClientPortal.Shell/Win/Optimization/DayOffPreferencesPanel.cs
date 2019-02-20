using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Optimization
{
    public partial class DayOffPreferencesPanel : BaseUserControl, IDataExchange
    {

        public IDaysOffPreferences DaysOffPreferences { get; private set; }

        public DayOffPreferencesPanel()
        {
            InitializeComponent();
        }

        public void Initialize(IDaysOffPreferences daysOffPreferences, bool useRightToLeft)
        {
			if (!useRightToLeft)
			{
				if (!DesignMode) SetTextsNoRightToLeft();
			}
			else
			{
				if (!DesignMode) SetTexts();
			}
			DaysOffPreferences = daysOffPreferences;

            ExchangeData(ExchangeDataOption.ServerToClient);
            setInitialControlStatus();
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
            if (direction == ExchangeDataOption.ServerToClient)
            {
                setFormData();
            }
            else
            {
                getFormData();
            }
        }

        #endregion

        private bool validateRuleSet()
        {
            bool ret = true;
            if (!validateMinMax(checkBoxDaysOffPerWeek, numericUpDownDaysOffPerWeekMin, numericUpDownDaysOffPerWeekMax, UserTexts.Resources.MinDaysOffPerWeekMustBeLowerOrEqualToMax))
                ret = false;

            if (!validateMinMax(checkBoxConsecutiveDaysOff, numericUpDownConsDayOffMin, numericUpDownConsDayOffMax, UserTexts.Resources.ConsecutiveDaysOffPerWeekMustBeLowerOrEqualToMax))
                ret = false;

            if (!validateMinMax(checkBoxConsecutiveWorkDays, numericUpDownConsWorkDaysMin, numericUpDownConsWorkDaysMax, UserTexts.Resources.ConsecutiveWorkdaysPerWeekMustBeLowerOrEqualToMax))
                ret = false;

            if (!validateMinMax(checkBoxFullWeekEndsOff, numericUpDownFullWeekendsOffMin, numericUpDownFullWeekendsOffMax, UserTexts.Resources.UseFreeWeekEnds))
                ret = false;

            if (!validateMinMax(checkBoxWeekEndDaysOff, numericUpDownWeekEndDaysOffMin, numericUpDownWeekEndDaysOffMax, UserTexts.Resources.UseFreeWeekEndDays))
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
                    ViewBase.ShowErrorMessage(this, message, UserTexts.Resources.DayOffReoptimizerOptions);

                    return false;
                }
            }

            return true;
        }

        private void getFormData()
        {
            DaysOffPreferences.UseKeepExistingDaysOff = checkBoxKeepExistingDaysOff.Checked;
            DaysOffPreferences.KeepExistingDaysOffValue = (double)numericUpDownKeepExistingDaysOff.Value / 100;
            
                DaysOffPreferences.UseDaysOffPerWeek = checkBoxDaysOffPerWeek.Checked;
            DaysOffPreferences.UseConsecutiveDaysOff = checkBoxConsecutiveDaysOff.Checked;
            DaysOffPreferences.UseConsecutiveWorkdays = checkBoxConsecutiveWorkDays.Checked;
            DaysOffPreferences.UseFullWeekendsOff = checkBoxFullWeekEndsOff.Checked;
            DaysOffPreferences.UseWeekEndDaysOff = checkBoxWeekEndDaysOff.Checked;

            DaysOffPreferences.DaysOffPerWeekValue = new MinMax<int>((int)numericUpDownDaysOffPerWeekMin.Value, (int)numericUpDownDaysOffPerWeekMax.Value);
            DaysOffPreferences.ConsecutiveDaysOffValue = new MinMax<int>((int)numericUpDownConsDayOffMin.Value, (int)numericUpDownConsDayOffMax.Value);
            DaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>((int)numericUpDownConsWorkDaysMin.Value, (int)numericUpDownConsWorkDaysMax.Value);
            DaysOffPreferences.WeekEndDaysOffValue = new MinMax<int>((int)numericUpDownWeekEndDaysOffMin.Value, (int)numericUpDownWeekEndDaysOffMax.Value);
            DaysOffPreferences.FullWeekendsOffValue = new MinMax<int>((int)numericUpDownFullWeekendsOffMin.Value, (int)numericUpDownFullWeekendsOffMax.Value);
			
            DaysOffPreferences.ConsiderWeekBefore = checkBoxConsiderWeekBefore.Checked;
            DaysOffPreferences.ConsiderWeekAfter = checkBoxConsiderWeekAfter.Checked;
        }

        private void setFormData()
        {
            checkBoxKeepExistingDaysOff.Checked = DaysOffPreferences.UseKeepExistingDaysOff;
            numericUpDownKeepExistingDaysOff.Value =  (decimal)DaysOffPreferences.KeepExistingDaysOffValue * 100;

            checkBoxDaysOffPerWeek.Checked = DaysOffPreferences.UseDaysOffPerWeek;
            checkBoxConsecutiveDaysOff.Checked = DaysOffPreferences.UseConsecutiveDaysOff;
            checkBoxConsecutiveWorkDays.Checked = DaysOffPreferences.UseConsecutiveWorkdays;
            checkBoxFullWeekEndsOff.Checked = DaysOffPreferences.UseFullWeekendsOff;
            checkBoxWeekEndDaysOff.Checked = DaysOffPreferences.UseWeekEndDaysOff;

            numericUpDownDaysOffPerWeekMin.Value = DaysOffPreferences.DaysOffPerWeekValue.Minimum;
            numericUpDownDaysOffPerWeekMax.Value = DaysOffPreferences.DaysOffPerWeekValue.Maximum;

            numericUpDownConsDayOffMin.Value = DaysOffPreferences.ConsecutiveDaysOffValue.Minimum;
            numericUpDownConsDayOffMax.Value = DaysOffPreferences.ConsecutiveDaysOffValue.Maximum;

            numericUpDownConsWorkDaysMin.Value = DaysOffPreferences.ConsecutiveWorkdaysValue.Minimum;
            numericUpDownConsWorkDaysMax.Value = DaysOffPreferences.ConsecutiveWorkdaysValue.Maximum;

            numericUpDownFullWeekendsOffMin.Value = DaysOffPreferences.FullWeekendsOffValue.Minimum;
            numericUpDownFullWeekendsOffMax.Value = DaysOffPreferences.FullWeekendsOffValue.Maximum;

            numericUpDownWeekEndDaysOffMin.Value = DaysOffPreferences.WeekEndDaysOffValue.Minimum;
            numericUpDownWeekEndDaysOffMax.Value = DaysOffPreferences.WeekEndDaysOffValue.Maximum;

            checkBoxConsiderWeekBefore.Checked = DaysOffPreferences.ConsiderWeekBefore;
            checkBoxConsiderWeekAfter.Checked = DaysOffPreferences.ConsiderWeekAfter;

        }

        private void checkBoxDaysOffPerWeek_CheckedChanged(object sender, EventArgs e)
        {
            setNumericUpDownDaysOffPerWeekStatus();
        }

        private void setNumericUpDownDaysOffPerWeekStatus()
        {
            numericUpDownDaysOffPerWeekMin.Enabled = checkBoxDaysOffPerWeek.Checked;
            numericUpDownDaysOffPerWeekMax.Enabled = checkBoxDaysOffPerWeek.Checked;
        }

        private void checkBoxConsecutiveDaysOff_CheckedChanged(object sender, EventArgs e)
        {
            setNumericUpDownConsecutiveDaysOffStatus();
        }

        private void setNumericUpDownConsecutiveDaysOffStatus()
        {
            numericUpDownConsDayOffMin.Enabled = checkBoxConsecutiveDaysOff.Checked;
            numericUpDownConsDayOffMax.Enabled = checkBoxConsecutiveDaysOff.Checked;
        }

        private void checkBoxConsecutiveWorkDays_CheckedChanged(object sender, EventArgs e)
        {
            setNumericUpDownConsWorkDaysStatus();
        }

        private void setNumericUpDownConsWorkDaysStatus()
        {
            numericUpDownConsWorkDaysMin.Enabled = checkBoxConsecutiveWorkDays.Checked;
            numericUpDownConsWorkDaysMax.Enabled = checkBoxConsecutiveWorkDays.Checked;
        }

        private void checkBoxFullWeekEndsOff_CheckedChanged(object sender, EventArgs e)
        {
            setNumericUpDownFullWeekEndsOffStatus();
        }

        private void setNumericUpDownFullWeekEndsOffStatus()
        {
            numericUpDownFullWeekendsOffMin.Enabled = checkBoxFullWeekEndsOff.Checked;
            numericUpDownFullWeekendsOffMax.Enabled = checkBoxFullWeekEndsOff.Checked;
        }

        private void checkBoxWeekEndDaysOff_CheckedChanged(object sender, EventArgs e)
        {
            setNumericUpDownWeekEndDaysOffStatus();
        }

        private void setNumericUpDownWeekEndDaysOffStatus()
        {
            numericUpDownWeekEndDaysOffMin.Enabled = checkBoxWeekEndDaysOff.Checked;
            numericUpDownWeekEndDaysOffMax.Enabled = checkBoxWeekEndDaysOff.Checked;
        }

        private void checkBoxKeepExistingDaysOff_CheckedChanged(object sender, EventArgs e)
        {
            setNumericUpDownKeepExistingDaysOffStatus();
        }

        private void setNumericUpDownKeepExistingDaysOffStatus()
        {
            numericUpDownKeepExistingDaysOff.Enabled = checkBoxKeepExistingDaysOff.Checked;
        }

        private void setInitialControlStatus()
        {
            setNumericUpDownKeepExistingDaysOffStatus();

            setNumericUpDownDaysOffPerWeekStatus();
            setNumericUpDownConsecutiveDaysOffStatus();
            setNumericUpDownConsWorkDaysStatus();
            setNumericUpDownFullWeekEndsOffStatus();
            setNumericUpDownWeekEndDaysOffStatus();
        }

		private void numericUpDownDaysOffPerWeekMinValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownDaysOffPerWeekMin.Value > numericUpDownDaysOffPerWeekMax.Value) numericUpDownDaysOffPerWeekMax.Value = numericUpDownDaysOffPerWeekMin.Value;
		}

		private void numericUpDownDaysOffPerWeekMaxValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownDaysOffPerWeekMax.Value < numericUpDownDaysOffPerWeekMin.Value) numericUpDownDaysOffPerWeekMin.Value = numericUpDownDaysOffPerWeekMax.Value;
		}

		private void numericUpDownConsDayOffMinValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownConsDayOffMin.Value > numericUpDownConsDayOffMax.Value) numericUpDownConsDayOffMax.Value = numericUpDownConsDayOffMin.Value;
		}

		private void numericUpDownConsDayOffMaxValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownConsDayOffMax.Value < numericUpDownConsDayOffMin.Value) numericUpDownConsDayOffMin.Value = numericUpDownConsDayOffMax.Value;
		}

		private void numericUpDownConsWorkDaysMinValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownConsWorkDaysMin.Value > numericUpDownConsWorkDaysMax.Value) numericUpDownConsWorkDaysMax.Value = numericUpDownConsWorkDaysMin.Value;
		}

		private void numericUpDownConsWorkDaysMaxValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownConsWorkDaysMax.Value < numericUpDownConsWorkDaysMin.Value) numericUpDownConsWorkDaysMin.Value = numericUpDownConsWorkDaysMax.Value;
		}

		private void numericUpDownFullWeekendsOffMinValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownFullWeekendsOffMin.Value > numericUpDownFullWeekendsOffMax.Value) numericUpDownFullWeekendsOffMax.Value = numericUpDownFullWeekendsOffMin.Value;
		}

		private void numericUpDownFullWeekendsOffMaxValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownFullWeekendsOffMax.Value < numericUpDownFullWeekendsOffMin.Value) numericUpDownFullWeekendsOffMin.Value = numericUpDownFullWeekendsOffMax.Value;
		}

		private void numericUpDownWeekEndDaysOffMinValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownWeekEndDaysOffMin.Value > numericUpDownWeekEndDaysOffMax.Value) numericUpDownWeekEndDaysOffMax.Value = numericUpDownWeekEndDaysOffMin.Value;
		}

		private void numericUpDownWeekEndDaysOffMaxValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownWeekEndDaysOffMax.Value < numericUpDownWeekEndDaysOffMin.Value) numericUpDownWeekEndDaysOffMin.Value = numericUpDownWeekEndDaysOffMax.Value;
		}
    }
}