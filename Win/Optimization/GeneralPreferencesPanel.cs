﻿using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
    public partial class GeneralPreferencesPanel : BaseUserControl, IDataExchange
    {
        private IGeneralPreferences _generalPreferences;
        private IList<IScheduleTag> _scheduleTags;

        public GeneralPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void Initialize(
            IGeneralPreferences generalPreferences, 
            IList<IScheduleTag> scheduleTags)
        {
            _generalPreferences = generalPreferences;
            _scheduleTags = scheduleTags;

            ExchangeData(ExchangeDataOption.DataSourceToControls);
            setInitialControlStatus();
        }

        #region IDataExchange Members

        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
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
