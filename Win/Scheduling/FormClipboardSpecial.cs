using System;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Clipboard;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class FormClipboardSpecial : BaseRibbonForm, IClipboardSpecialView
    {
        private readonly ClipboardSpecialPresenter _clipboardSpecialPresenter;

        public FormClipboardSpecial(bool deleteMode, bool showRestrictions, PasteOptions pasteOptions, bool showOvertimeAvailability)
        {
            InitializeComponent();
  
            _clipboardSpecialPresenter = new ClipboardSpecialPresenter(this, pasteOptions, deleteMode, showRestrictions);
            _clipboardSpecialPresenter.Initialize();

            checkBoxOvertimeAvailability.Visible = showOvertimeAvailability;
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
    }
}
