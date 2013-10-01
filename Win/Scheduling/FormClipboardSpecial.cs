using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class FormClipboardSpecial : BaseRibbonForm, IClipboardSpecialView
    {
        private readonly ClipboardSpecialPresenter _clipboardSpecialPresenter;

		public FormClipboardSpecial(PasteOptions pasteOptions, ClipboardSpecialOptions clipboardSpecialOptions, IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSet)
        {
            InitializeComponent();
  
            _clipboardSpecialPresenter = new ClipboardSpecialPresenter(this, pasteOptions, clipboardSpecialOptions.DeleteMode, clipboardSpecialOptions.ShowRestrictions);
            _clipboardSpecialPresenter.Initialize();

	        checkBoxOvertimeAvailability.Visible = clipboardSpecialOptions.ShowOvertimeAvailability;
	        checkBoxShiftAsOvertime.Visible = clipboardSpecialOptions.ShowShiftAsOvertime;
	        comboBoxAdvOvertime.Visible = clipboardSpecialOptions.ShowShiftAsOvertime;
			fillComboOvertime(multiplicatorDefinitionSet);
			enableComboBoxOvertime();
        }

		private void fillComboOvertime(IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSet)
		{
			comboBoxAdvOvertime.DisplayMember = "Name";

			var definitionSets = from set in MultiplicatorDefinitionSet
								 where set.MultiplicatorType == MultiplicatorType.Overtime
								 select set;


			comboBoxAdvOvertime.DataSource = definitionSets.ToArray();
		}

        public void ShowRestrictions(bool show)
        {
            checkBoxPreferences.Visible = show;
            checkBoxStudentAvailability.Visible = show;
        }

        public bool Cancel()
        {
            return _clipboardSpecialPresenter.IsCanceled();
        }

        public void SetColor()
        {
            this.BackColor = ColorHelper.FormBackgroundColor();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnButtonOkClick();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnButtonCancelClick();
        }

        public void HideForm()
        {
            this.Hide();
        }

        public void SetPermissionOnAbsences(bool permission)
        {
            checkBoxAbsences.Enabled = permission;
        }

        public void SetPermissionOnDayOffs(bool permission)
        {
            checkBoxDayOffs.Enabled = permission;
        }

        public void SetPermissionOnPersonalAssignments(bool permission)
        {
            checkBoxPersonalAssignments.Enabled = permission;
        }

        public void SetPermissionOnAssignments(bool permission)
        {
            checkBoxAssignments.Enabled = permission;
        }

        public void SetPermissionOnOvertime(bool permission)
        {
            checkBoxOvertime.Enabled = permission;
        }

        public void SetPermissionsOnRestrictions(bool permission)
        {
            checkBoxPreferences.Enabled = permission;
            checkBoxStudentAvailability.Enabled = permission;
        }

		public void SetPermissionsOnShiftAsOvertime(bool permission)
		{
			checkBoxShiftAsOvertime.Enabled = permission;
			comboBoxAdvOvertime.Enabled = permission;
		}

        private void checkBoxAssignments_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxAssignmentsCheckedChanged(checkBoxAssignments.Checked);
        }

        private void checkBoxAbsences_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxAbsencesCheckedChanged(checkBoxAbsences.Checked);
        }

        private void checkBoxDayOffs_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxDayOffsCheckedChanged(checkBoxDayOffs.Checked);
        }

        private void checkBoxPersonalAssignments_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxPersonalAssignmentsCheckedChanged(checkBoxPersonalAssignments.Checked);
        }

        private void checkBoxOvertime_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxOvertimeCheckedChanged(checkBoxOvertime.Checked);
        }

        private void FormClipboardSpecial_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                _clipboardSpecialPresenter.OnFormClosing();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
        }

        private void checkBoxPreferences_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxPreferencesCheckedChanged(checkBoxPreferences.Checked);
        }

        private void checkBoxStudentAvailability_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxStudentAvailabilityCheckedChange(checkBoxStudentAvailability.Checked);
        }

        private void checkBoxOvertimeAvailability_CheckedChanged(object sender, EventArgs e)
        {
            _clipboardSpecialPresenter.OnCheckBoxOvertimeAvailabilityCheckedChanged(checkBoxOvertimeAvailability.Checked);
        }

		private void checkBoxShiftAsOvertime_CheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxShiftAsOvertimeCheckedChanged(checkBoxShiftAsOvertime.Checked);
			enableComboBoxOvertime();
		}

		private void comboBoxAdvOvertime_SelectedIndexChanged(object sender, EventArgs e)
		{
			var multiplicatorDefinitionSet = comboBoxAdvOvertime.SelectedItem as IMultiplicatorDefinitionSet;
			if(multiplicatorDefinitionSet != null)
				_clipboardSpecialPresenter.OnComboBoxAdvOvertimeSelectedIndexChanged(multiplicatorDefinitionSet);
		}

		private void enableComboBoxOvertime()
		{
			comboBoxAdvOvertime.Enabled = checkBoxShiftAsOvertime.Checked;
		}
    }

	public class ClipboardSpecialOptions
	{
		public bool DeleteMode { get; set; }
		public bool ShowRestrictions { get; set; }
		public bool ShowOvertimeAvailability { get; set; }
		public bool ShowShiftAsOvertime { get; set; }
	}
}
