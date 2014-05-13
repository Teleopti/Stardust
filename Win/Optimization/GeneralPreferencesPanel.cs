using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
    public partial class GeneralPreferencesPanel : BaseUserControl, IDataExchange
    {
        private IGeneralPreferences _generalPreferences;
        private IEnumerable<IScheduleTag> _scheduleTags;
    	private IEventAggregator _eventAggregator;

        public GeneralPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void Initialize(
            IGeneralPreferences generalPreferences, 
            IEnumerable<IScheduleTag> scheduleTags,
			IEventAggregator eventAggregator)
        {
            _generalPreferences = generalPreferences;
	        _scheduleTags = addKeepOriginalScheduleTag(scheduleTags);
        	_eventAggregator = eventAggregator;

			if(_eventAggregator != null)
			_eventAggregator.GetEvent<GenericEvent<ExtraPreferencesPanelUseBlockScheduling>>().Subscribe(enableDisableShiftCategoryLimitations);

            ExchangeData(ExchangeDataOption.DataSourceToControls);
            setInitialControlStatus();
        }

        #region IDataExchange Members

		public void UnsubscribeEvents()
		{
			if (_eventAggregator == null) return;
			_eventAggregator.GetEvent<GenericEvent<ExtraPreferencesPanelUseBlockScheduling>>().Unsubscribe(enableDisableShiftCategoryLimitations); 
		}

		private void enableDisableShiftCategoryLimitations(EventParameters<ExtraPreferencesPanelUseBlockScheduling> obj)
		{
			checkBoxShiftCategoryLimitations.Enabled = !obj.Value.Use;
			if (!obj.Value.Use) return;
			checkBoxShiftCategoryLimitations.Checked = false;
			_generalPreferences.UseShiftCategoryLimitations = false;
		}

    	public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        public bool IsOptimizationStepsChecked()
        {
            if ( checkBoxShiftsForFlexibleWorkTime.Checked ||
                checkBoxDaysOffFromFlexibleWorkTime.Checked || checkBoxShiftCategoryLimitations.Checked)
                return true;
            return false;
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.DataSourceToControls)
            {
                bindTagsCombo();

                comboBoxAdvTag.SelectedItem = _generalPreferences.ScheduleTag;
                checkBoxDaysOff.Checked = _generalPreferences.OptimizationStepDaysOff;
				checkBoxTimeBetweenDays.Checked = _generalPreferences.OptimizationStepTimeBetweenDays;
                checkBoxShiftsForFlexibleWorkTime.Checked = _generalPreferences.OptimizationStepShiftsForFlexibleWorkTime;
                checkBoxDaysOffFromFlexibleWorkTime.Checked = _generalPreferences.OptimizationStepDaysOffForFlexibleWorkTime;
				checkBoxShiftsWithinDay.Checked = _generalPreferences.OptimizationStepShiftsWithinDay;
				checkBoxFairness.Checked = _generalPreferences.OptimizationStepFairness;

                checkBoxPreferences.Checked = _generalPreferences.UsePreferences;
                checkBoxMustHaves.Checked = _generalPreferences.UseMustHaves;
                checkBoxRotations.Checked = _generalPreferences.UseRotations;
                checkBoxAvailabilities.Checked = _generalPreferences.UseAvailabilities;
                checkBoxStudentAvailabilities.Checked = _generalPreferences.UseStudentAvailabilities;
                checkBoxShiftCategoryLimitations.Checked = _generalPreferences.UseShiftCategoryLimitations;

                numericUpDownPreferences.Value = (decimal)_generalPreferences.PreferencesValue * 100;
                numericUpDownMustHaves.Value = (decimal)_generalPreferences.MustHavesValue * 100;
                numericUpDownRotations.Value = (decimal) _generalPreferences.RotationsValue * 100;
                numericUpDownAvailabilities.Value = (decimal)_generalPreferences.AvailabilitiesValue * 100;
                numericUpDownStudentAvailabilities.Value = (decimal)_generalPreferences.StudentAvailabilitiesValue * 100;
            }
            else
            {
                _generalPreferences.ScheduleTag = (IScheduleTag)comboBoxAdvTag.SelectedItem;

                _generalPreferences.OptimizationStepDaysOff = checkBoxDaysOff.Checked;
				_generalPreferences.OptimizationStepShiftsWithinDay = checkBoxShiftsWithinDay.Checked;
                _generalPreferences.OptimizationStepShiftsForFlexibleWorkTime = checkBoxShiftsForFlexibleWorkTime.Checked;
                _generalPreferences.OptimizationStepDaysOffForFlexibleWorkTime = checkBoxDaysOffFromFlexibleWorkTime.Checked;
				_generalPreferences.OptimizationStepTimeBetweenDays = checkBoxTimeBetweenDays.Checked;
				_generalPreferences.OptimizationStepFairness = checkBoxFairness.Checked;

                _generalPreferences.UsePreferences = checkBoxPreferences.Checked;
                _generalPreferences.UseMustHaves = checkBoxMustHaves.Checked;
                _generalPreferences.UseRotations = checkBoxRotations.Checked;
                _generalPreferences.UseAvailabilities = checkBoxAvailabilities.Checked;
                _generalPreferences.UseStudentAvailabilities = checkBoxStudentAvailabilities.Checked;
                _generalPreferences.UseShiftCategoryLimitations = checkBoxShiftCategoryLimitations.Checked;

                _generalPreferences.PreferencesValue = (double)numericUpDownPreferences.Value / 100;
                _generalPreferences.MustHavesValue = (double)numericUpDownMustHaves.Value / 100;
                _generalPreferences.RotationsValue = (double)numericUpDownRotations.Value / 100;
                _generalPreferences.AvailabilitiesValue = (double)numericUpDownAvailabilities.Value / 100;
                _generalPreferences.StudentAvailabilitiesValue = (double)numericUpDownStudentAvailabilities.Value / 100;


            }
        }

        #endregion

		 private IEnumerable<IScheduleTag> addKeepOriginalScheduleTag(IEnumerable<IScheduleTag> scheduleTags)
		 {
			 var list = scheduleTags.ToList();
			var keepOriginalScheduleTag = KeepOriginalScheduleTag.Instance;
			list.Insert(1, keepOriginalScheduleTag);
			 return list;
		 }

        private void bindTagsCombo()
        {
            comboBoxAdvTag.DataSource = _scheduleTags;
            comboBoxAdvTag.DisplayMember = "Description";
            comboBoxAdvTag.SelectedIndex = 0;
        }

        private void checkBoxPreferences_CheckedChanged(object sender, System.EventArgs e)
        {
            setNumericUpDownPreferencesStatus();
        }

        private void setNumericUpDownPreferencesStatus()
        {
            numericUpDownPreferences.Enabled = checkBoxPreferences.Checked;
        }

        private void checkBoxMustHaves_CheckedChanged(object sender, System.EventArgs e)
        {
            setNumericUpDownMustHavesStatus();
        }

        private void setNumericUpDownMustHavesStatus()
        {
            numericUpDownMustHaves.Enabled = checkBoxMustHaves.Checked;
        }

        private void checkBoxRotations_CheckedChanged(object sender, System.EventArgs e)
        {
            setNumericUpDownRotationsStatus();
        }

        private void setNumericUpDownRotationsStatus()
        {
            numericUpDownRotations.Enabled = checkBoxRotations.Checked;
        }

        private void checkBoxAvailabilities_CheckedChanged(object sender, System.EventArgs e)
        {
            setNumericUpDownAvailabilitiesStatus();
        }

        private void setNumericUpDownAvailabilitiesStatus()
        {
            numericUpDownAvailabilities.Enabled = checkBoxAvailabilities.Checked;
        }

        private void checkBoxStudentAvailabilities_CheckedChanged(object sender, System.EventArgs e)
        {
            setNumericUpDownStudentAvailabilitiesStatus();
        }

        private void setNumericUpDownStudentAvailabilitiesStatus()
        {
            numericUpDownStudentAvailabilities.Enabled = checkBoxStudentAvailabilities.Checked;
        }

        private void setInitialControlStatus()
        {
            setNumericUpDownPreferencesStatus();
            setNumericUpDownMustHavesStatus();
            setNumericUpDownRotationsStatus();
            setNumericUpDownAvailabilitiesStatus();
            setNumericUpDownStudentAvailabilitiesStatus();
        }

       
    }

}
