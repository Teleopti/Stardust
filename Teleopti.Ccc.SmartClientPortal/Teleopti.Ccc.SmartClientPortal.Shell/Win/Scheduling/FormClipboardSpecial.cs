using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class FormClipboardSpecial : BaseDialogForm, IClipboardSpecialView
	{
		private readonly ClipboardSpecialPresenter _clipboardSpecialPresenter;

		public FormClipboardSpecial(PasteOptions pasteOptions, ClipboardSpecialOptions clipboardSpecialOptions, IEnumerable<IMultiplicatorDefinitionSet> multiplicatorDefinitionSet, bool useRightToLeft)
		{
			InitializeComponent();
  
			_clipboardSpecialPresenter = new ClipboardSpecialPresenter(this, pasteOptions, clipboardSpecialOptions.DeleteMode, clipboardSpecialOptions.ShowRestrictions);
			_clipboardSpecialPresenter.Initialize(useRightToLeft);

			checkBoxOvertimeAvailability.Visible = clipboardSpecialOptions.ShowOvertimeAvailability;
			checkBoxShiftAsOvertime.Visible = clipboardSpecialOptions.ShowShiftAsOvertime;
			comboBoxAdvOvertime.Visible = clipboardSpecialOptions.ShowShiftAsOvertime;
			fillComboOvertime(multiplicatorDefinitionSet);
			enableComboBoxOvertime();
		}

		private void fillComboOvertime(IEnumerable<IMultiplicatorDefinitionSet> multiplicatorDefinitionSet)
		{
			comboBoxAdvOvertime.DisplayMember = "Name";

			var definitionSets = from set in multiplicatorDefinitionSet
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
			BackColor = ColorHelper.FormBackgroundColor();
		}

		private void buttonOkClick(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnButtonOkClick();
		}

		private void buttonCancelClick(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnButtonCancelClick();
		}

		public void HideForm()
		{
			Close();
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

		private void checkBoxAssignmentsCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxAssignmentsCheckedChanged(checkBoxAssignments.Checked);
		}

		private void checkBoxAbsencesCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxAbsencesCheckedChanged(checkBoxAbsences.Checked);
		}

		private void checkBoxDayOffsCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxDayOffsCheckedChanged(checkBoxDayOffs.Checked);
		}

		private void checkBoxPersonalAssignmentsCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxPersonalAssignmentsCheckedChanged(checkBoxPersonalAssignments.Checked);
		}

		private void checkBoxOvertimeCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxOvertimeCheckedChanged(checkBoxOvertime.Checked);
		}

		private void checkBoxPreferencesCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxPreferencesCheckedChanged(checkBoxPreferences.Checked);
		}

		private void checkBoxStudentAvailabilityCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxStudentAvailabilityCheckedChange(checkBoxStudentAvailability.Checked);
		}

		private void checkBoxOvertimeAvailabilityCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxOvertimeAvailabilityCheckedChanged(checkBoxOvertimeAvailability.Checked);
		}

		private void checkBoxShiftAsOvertimeCheckedChanged(object sender, EventArgs e)
		{
			_clipboardSpecialPresenter.OnCheckBoxShiftAsOvertimeCheckedChanged(checkBoxShiftAsOvertime.Checked);
			enableComboBoxOvertime();
		}

		private void comboBoxAdvOvertimeSelectedIndexChanged(object sender, EventArgs e)
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
