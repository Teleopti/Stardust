using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingSessionPreferences
{
	public partial class SchedulingSessionPreferencesTabPanel : BaseUserControl, IDataExchange
	{
		private SchedulingOptions _localSchedulingOptions;
		private SchedulingOptions _schedulingOptions;
		private IEnumerable<IShiftCategory> _shiftCategories;
		private bool _dataLoaded;
		private IList<GroupPageLight> _groupPages;
		private IEnumerable<IActivity> _availableActivity;
		private IEnumerable<IScheduleTag> _scheduleTags;
		private SchedulerGroupPagesProvider _groupPagesProvider;
		private GroupPageLight _singleAgentEntry;

		public SchedulingSessionPreferencesTabPanel()
		{
			InitializeComponent();
			//if (!DesignMode) SetTexts();
		}

		public void Initialize(SchedulingOptions schedulingOptions, IEnumerable<IShiftCategory> shiftCategories, SchedulerGroupPagesProvider groupPagesProvider, IEnumerable<IScheduleTag> scheduleTags, IEnumerable<IActivity> availableActivity, bool useRightToLeft)
		{
			if (!useRightToLeft)
			{
				if (!DesignMode) SetTextsNoRightToLeft();
			}
			else
			{
				if (!DesignMode) SetTexts();
			}
			_groupPagesProvider = groupPagesProvider;
			_availableActivity = availableActivity;

			tableLayoutPanel2.RowStyles[2].Height = 0;
			tableLayoutPanel2.RowStyles[1].Height = 0;
			_schedulingOptions = schedulingOptions;
			_shiftCategories = shiftCategories;
			_scheduleTags = scheduleTags;
			_groupPages = _groupPagesProvider.GetGroups(false);
			_singleAgentEntry = GroupPageLight.SingleAgentGroup(Resources.NoTeam);
			ExchangeData(ExchangeDataOption.DataSourceToControls);
			_dataLoaded = true;
		}

		public override string HelpId
		{
			get { return Tag.ToString(); }
		}

		public bool ScheduleOnlyRotationDaysVisible
		{
			get { return checkBoxOnlyRotationDays.Visible; }
			set { checkBoxOnlyRotationDays.Visible = value; }
		}

		public bool ScheduleOnlyAvailableDaysVisible
		{
			get { return checkBoxOnlyAvailabilityDays.Visible; }
			set { checkBoxOnlyAvailabilityDays.Visible = value; }
		}

		public bool ScheduleOnlyPreferenceDaysVisible
		{
			get { return checkBoxOnlyPreferenceDays.Visible; }
			set { checkBoxOnlyPreferenceDays.Visible = value; }
		}

		public bool ShiftCategoryVisible
		{
			get { return pnlShiftCategory.Visible; }
			set { pnlShiftCategory.Visible = value; }
		}


		public bool UseGroupSchedulingCommonStart
		{
			get { return checkBoxTeamSameStartTime.Checked; }
			set { checkBoxTeamSameStartTime.Checked = value; }
		}

		public bool UseGroupSchedulingCommonEnd
		{
			get { return checkBoxTeamSameEndTime.Checked; }
			set { checkBoxTeamSameEndTime.Checked = value; }
		}

		public bool UseGroupSchedulingCommonCategory
		{
			get { return checkBoxTeamSameShiftCategory.Checked; }
			set { checkBoxTeamSameShiftCategory.Checked = value; }
		}

		public bool UseCommonActivity
		{
			get { return checkBoxTeamSameActivity.Checked; }
			set { checkBoxTeamSameActivity.Checked = value; }
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
				dataOffline();
				initShiftCategories();
				initGroupPages();
				initBlockType();
				initCommonActivity();
				initTags();
				setDataInControls();
			}
			else
			{
				getDataFromControls();
				dataOnline();
			}
		}

		#endregion

	    private void initShiftCategories()
		{
			if (_shiftCategories != null)
			{
				comboBoxAdvShiftCategory.DataSource = null;
				var shiftCategoryBindingList = new TypedBindingCollection<IShiftCategory>();
				foreach (var item in _shiftCategories)
				{
					shiftCategoryBindingList.Add(item);
				}
				comboBoxAdvShiftCategory.DataSource = shiftCategoryBindingList;
				comboBoxAdvShiftCategory.DisplayMember = "Description";
				comboBoxAdvShiftCategory.SelectedIndex = 0;
			}
		}

		private void initBlockType()
		{
			comboBoxBlockType.DisplayMember = "Value";
			comboBoxBlockType.ValueMember = "Key";
			comboBoxBlockType.DataSource = LanguageResourceHelper.TranslateEnumToList<BlockFinderType>();
			comboBoxBlockType.SelectedIndex = 0;
		}

		private void initGroupPages()
		{
			var tempGroupPages = _groupPages;
			tempGroupPages.Insert(0, _singleAgentEntry);
			comboBoxTeamGroupPage.DataSource = tempGroupPages;
			comboBoxTeamGroupPage.DisplayMember = "DisplayName";
			comboBoxTeamGroupPage.SelectedIndex = 0;
		}

		private void initCommonActivity()
		{
			comboBoxTeamActivity.DataSource = _availableActivity;
			comboBoxTeamActivity.DisplayMember = "Name";
			comboBoxTeamActivity.ValueMember = "Name";
			if (_localSchedulingOptions.CommonActivity != null)
			{
				comboBoxTeamActivity.SelectedValue = _localSchedulingOptions.CommonActivity.Name;
			}
		}

		private void initTags()
		{
			comboBoxAdvTag.DataSource = _scheduleTags;
			comboBoxAdvTag.DisplayMember = "Description";
			comboBoxAdvTag.SelectedItem = _localSchedulingOptions.TagToUseOnScheduling;
		}

		private void dataOffline()
		{
			_localSchedulingOptions = _schedulingOptions.Clone();
		}

		private void dataOnline()
		{
			_schedulingOptions.UseRotations = _localSchedulingOptions.UseRotations;
			_schedulingOptions.RotationDaysOnly = _localSchedulingOptions.RotationDaysOnly;
			_schedulingOptions.AvailabilityDaysOnly = _localSchedulingOptions.AvailabilityDaysOnly;
			_schedulingOptions.PreferencesDaysOnly = _localSchedulingOptions.PreferencesDaysOnly;
			_schedulingOptions.UseMaximumStaffing = _localSchedulingOptions.UseMaximumStaffing;
			_schedulingOptions.UseMinimumStaffing = _localSchedulingOptions.UseMinimumStaffing;
			_schedulingOptions.UseAvailability = _localSchedulingOptions.UseAvailability;
			_schedulingOptions.UseStudentAvailability = _localSchedulingOptions.UseStudentAvailability;
			_schedulingOptions.UsePreferences = _localSchedulingOptions.UsePreferences;
			_schedulingOptions.ShiftCategory = _localSchedulingOptions.ShiftCategory;
			_schedulingOptions.RefreshRate = _localSchedulingOptions.RefreshRate;
			_schedulingOptions.UseShiftCategoryLimitations = _localSchedulingOptions.UseShiftCategoryLimitations;
			_schedulingOptions.UsePreferencesMustHaveOnly = _localSchedulingOptions.UsePreferencesMustHaveOnly;
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling =
				_localSchedulingOptions.BlockFinderTypeForAdvanceScheduling;
			_schedulingOptions.UseTeam = _localSchedulingOptions.UseTeam;
			_schedulingOptions.GroupOnGroupPageForTeamBlockPer = _localSchedulingOptions.GroupOnGroupPageForTeamBlockPer;
			_schedulingOptions.UseSameDayOffs = _localSchedulingOptions.UseSameDayOffs;
			_schedulingOptions.TagToUseOnScheduling = _localSchedulingOptions.TagToUseOnScheduling;
			_schedulingOptions.ShowTroubleshot = _localSchedulingOptions.ShowTroubleshot;
		    _schedulingOptions.TeamSameShiftCategory = 
				_localSchedulingOptions.TeamSameShiftCategory;
			_schedulingOptions.TeamSameEndTime = _localSchedulingOptions.TeamSameEndTime;
			_schedulingOptions.TeamSameStartTime = _localSchedulingOptions.TeamSameStartTime;
			_schedulingOptions.TeamSameActivity = _localSchedulingOptions.TeamSameActivity;
			_schedulingOptions.CommonActivity = _localSchedulingOptions.CommonActivity;
			_schedulingOptions.UseAverageShiftLengths = _localSchedulingOptions.UseAverageShiftLengths;
			_schedulingOptions.BlockSameEndTime = _localSchedulingOptions.BlockSameEndTime;
			_schedulingOptions.BlockSameShift = _localSchedulingOptions.BlockSameShift;
			_schedulingOptions.BlockSameShiftCategory = _localSchedulingOptions.BlockSameShiftCategory;
			_schedulingOptions.BlockSameStartTime = _localSchedulingOptions.BlockSameStartTime;
			_schedulingOptions.UseBlock = _localSchedulingOptions.UseBlock;

		}

		private void getDataFromControls()
		{
			_localSchedulingOptions.RefreshRate = (int) numericUpDownRefreshRate.Value;
			_localSchedulingOptions.UseRotations = checkBoxUseRotations.Checked;
			_localSchedulingOptions.RotationDaysOnly = checkBoxOnlyRotationDays.Checked;
			_localSchedulingOptions.UseAvailability = checkBoxUseAvailability.Checked;
			_localSchedulingOptions.AvailabilityDaysOnly = checkBoxOnlyAvailabilityDays.Checked;
			_localSchedulingOptions.UseStudentAvailability = checkBoxUseStudentAvailability.Checked;
			_localSchedulingOptions.UseMaximumStaffing = checkBoxUseMaximumPersons.Checked;
			_localSchedulingOptions.UseMinimumStaffing = checkBoxUseMinimumPersons.Checked;
			_localSchedulingOptions.UsePreferences = checkBoxUsePreferences.Checked;
			_localSchedulingOptions.PreferencesDaysOnly = checkBoxOnlyPreferenceDays.Checked;
			_localSchedulingOptions.UsePreferencesMustHaveOnly = checkBoxMustHaves.Checked;
			if (checkBoxUseShiftCategory.Checked)
				_localSchedulingOptions.ShiftCategory = (IShiftCategory) comboBoxAdvShiftCategory.SelectedItem;
			else
				_localSchedulingOptions.ShiftCategory = null;
			_localSchedulingOptions.UseShiftCategoryLimitations = checkBoxUseShiftCategoryRestrictions.Checked;
			_localSchedulingOptions.TagToUseOnScheduling = (IScheduleTag) comboBoxAdvTag.SelectedItem;
			_localSchedulingOptions.ShowTroubleshot = checkBoxShowTroubleShot.Checked;

			if (checkBoxTeamSameActivity.Checked)
				_localSchedulingOptions.CommonActivity = (IActivity) comboBoxTeamActivity.SelectedItem;
			_localSchedulingOptions.UseAverageShiftLengths = checkBoxUseAverageShiftLengths.Checked;
			getTeamBlockDataToSave();
		}

		private void setDataInControls()
		{
			numericUpDownRefreshRate.Value = _localSchedulingOptions.RefreshRate;

			checkBoxUseRotations.Checked = _localSchedulingOptions.UseRotations;

			checkBoxOnlyRotationDays.Checked = _localSchedulingOptions.RotationDaysOnly;
			checkBoxOnlyRotationDays.Enabled = _localSchedulingOptions.UseRotations;

			checkBoxUseAvailability.Checked = _localSchedulingOptions.UseAvailability;

			checkBoxOnlyAvailabilityDays.Checked = _localSchedulingOptions.AvailabilityDaysOnly;
			checkBoxOnlyAvailabilityDays.Enabled = _localSchedulingOptions.UseAvailability;

			checkBoxUseStudentAvailability.Checked = _localSchedulingOptions.UseStudentAvailability;

			checkBoxUsePreferences.Checked = _localSchedulingOptions.UsePreferences;

			checkBoxOnlyPreferenceDays.Checked = _localSchedulingOptions.PreferencesDaysOnly;
			checkBoxOnlyPreferenceDays.Enabled = _localSchedulingOptions.UsePreferences;

			checkBoxMustHaves.Checked = _localSchedulingOptions.UsePreferencesMustHaveOnly;
			checkBoxMustHaves.Enabled = _localSchedulingOptions.UsePreferences;

			checkBoxUseMaximumPersons.Checked = _localSchedulingOptions.UseMaximumStaffing;
			checkBoxUseMinimumPersons.Checked = _localSchedulingOptions.UseMinimumStaffing;

			checkBoxUseShiftCategoryRestrictions.Checked = _localSchedulingOptions.UseShiftCategoryLimitations;

			checkBoxUseShiftCategory.Checked = _localSchedulingOptions.ShiftCategory != null;
			checkBoxShowTroubleShot.Checked = _localSchedulingOptions.ShowTroubleshot;

			checkBoxUseAverageShiftLengths.Checked = _localSchedulingOptions.UseAverageShiftLengths;

			setTeamBlockDataToSave();

			if (_localSchedulingOptions.ShiftCategory != null)
				comboBoxAdvShiftCategory.SelectedItem = _localSchedulingOptions.ShiftCategory;

			for (int index = 0; index < comboBoxTeamGroupPage.Items.Count; index++)
			{
				if (
					((GroupPageLight) comboBoxTeamGroupPage.Items[index]).Equals(
						_localSchedulingOptions.GroupOnGroupPageForTeamBlockPer))
				{
					comboBoxTeamGroupPage.SelectedIndex = index;
					break;
				}
			}
			
			changeGrpSchedulingCommonOptionState(isTeamSelected());

			comboBoxAdvShiftCategory.Enabled = checkBoxUseShiftCategory.Checked;

			comboBoxBlockType.SelectedValue = _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling;

			teamBlockSchedulingSessionOptionsHandler();
			teamSchedulingSessionOptionsHandler();
		}

		private void checkBoxUseRotationsCheckedChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void checkBoxOnlyRotationDaysCheckedChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void checkBoxUseAvailabilityCheckedChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void checkBoxUseStudentAvailabilityCheckedChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void checkBoxUsePreferencesCheckedChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void comboBoxAdvShiftCategorySelectedIndexChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void checkBoxUseShiftCategoryCheckedChanged(object sender, EventArgs e)
		{
			comboBoxAdvShiftCategory.Enabled = checkBoxUseShiftCategory.Checked;
		}

		private void checkBoxOnlyAvailabilityDaysCheckedChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void checkBoxOnlyPreferenceDaysCheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxOnlyPreferenceDays.Checked) checkBoxMustHaves.Checked = false;
			exchangeIfDataLoaded();
		}

		private void checkBoxUseShiftCategoryRestrictionsCheckedChanged(object sender, EventArgs e)
		{
			exchangeIfDataLoaded();
		}

		private void checkBoxMustHavesCheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxMustHaves.Checked) checkBoxOnlyPreferenceDays.Checked = false;
			exchangeIfDataLoaded();
		}

		private void changeGrpSchedulingCommonOptionState(bool value)
		{
			checkBoxTeamSameShiftCategory.Enabled = value;
			checkBoxTeamSameEndTime.Enabled = value;
			checkBoxTeamSameStartTime.Enabled = value;
			checkBoxTeamSameActivity.Enabled = value;
			if (value && !(checkBoxTeamSameShiftCategory.Checked || checkBoxTeamSameStartTime.Checked ||
			               checkBoxTeamSameEndTime.Checked ||
			               checkBoxTeamSameActivity.Checked))
				checkBoxTeamSameShiftCategory.Checked = true;
		}

		private void comboBoxTeamGroupPageSelectedIndexChanged(object sender, EventArgs e)
		{
			changeGrpSchedulingCommonOptionState(isTeamSelected());
			exchangeIfDataLoaded();
		}

		private void exchangeIfDataLoaded()
		{
			if (_dataLoaded)
			{
				getDataFromControls();
				setDataInControls();
			}
		}

		private bool isTeamSelected()
		{
			return ((GroupPageLight) comboBoxTeamGroupPage.SelectedValue).Type != GroupPageType.SingleAgent;
		}

		private void tabControl1SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControl1.SelectedIndex == 0) Tag = "Main";
			if (tabControl1.SelectedIndex == 1) Tag = "Extra";
			if (tabControl1.SelectedIndex == 2) Tag = "Advanced";
		}

		public void HideTeamAndBlockSchedulingOptions()
		{
			var row = tableLayoutPanel2.GetRow(pnlBlockTeamScheduling);
			tableLayoutPanel2.Controls.Remove(pnlBlockTeamScheduling);
			tableLayoutPanel2.RowStyles.RemoveAt(row);
			foreach (Control control in tableLayoutPanel2.Controls)
			{
				var rowIndex = tableLayoutPanel2.GetRow(control);
				if (rowIndex > row)
					tableLayoutPanel2.SetRow(control, rowIndex - 1);
			}
			tabPageExtra.Hide();
		}

		private void getTeamBlockDataToSave()
		{
			_localSchedulingOptions.GroupOnGroupPageForTeamBlockPer = (GroupPageLight) comboBoxTeamGroupPage.SelectedItem;
			_localSchedulingOptions.UseTeam = isTeamSelected();

			_localSchedulingOptions.TeamSameShiftCategory = checkBoxTeamSameShiftCategory.Checked;
			_localSchedulingOptions.TeamSameStartTime = checkBoxTeamSameStartTime.Checked;
			_localSchedulingOptions.TeamSameEndTime = checkBoxTeamSameEndTime.Checked;
			_localSchedulingOptions.TeamSameActivity = checkBoxTeamSameActivity.Checked;

			//block
			_localSchedulingOptions.BlockFinderTypeForAdvanceScheduling = (BlockFinderType) comboBoxBlockType.SelectedValue;
			_localSchedulingOptions.UseBlock = ((BlockFinderType) comboBoxBlockType.SelectedValue != BlockFinderType.SingleDay);

			_localSchedulingOptions.BlockSameEndTime = false;
			_localSchedulingOptions.BlockSameShiftCategory = checkBoxBlockSameShiftCategory.Checked;
			_localSchedulingOptions.BlockSameStartTime = checkBoxBlockSameStartTime.Checked;
			_localSchedulingOptions.BlockSameShift = checkBoxBlockSameShift.Checked;
		}

		private void setTeamBlockDataToSave()
		{
			checkBoxTeamSameShiftCategory.Checked = _localSchedulingOptions.TeamSameShiftCategory;
			checkBoxTeamSameEndTime.Checked = _localSchedulingOptions.TeamSameEndTime;
			checkBoxTeamSameStartTime.Checked = _localSchedulingOptions.TeamSameStartTime;
			checkBoxTeamSameActivity.Checked = _localSchedulingOptions.TeamSameActivity;
			comboBoxTeamActivity.Enabled = _localSchedulingOptions.TeamSameActivity;

			checkBoxBlockSameShiftCategory.Checked = _localSchedulingOptions.BlockSameShiftCategory;
			checkBoxBlockSameStartTime.Checked = _localSchedulingOptions.BlockSameStartTime;
			checkBoxBlockSameShift.Checked = _localSchedulingOptions.BlockSameShift;
		}

		private void comboBoxBlockType_SelectedValueChanged(object sender, EventArgs e)
		{
			if (comboBoxBlockType.SelectedValue == null)
				return;
			var isEnabled = (BlockFinderType) comboBoxBlockType.SelectedValue != BlockFinderType.SingleDay;
			checkBoxBlockSameStartTime.Enabled = isEnabled;
			checkBoxBlockSameShift.Enabled = isEnabled;
			checkBoxBlockSameShiftCategory.Enabled = isEnabled;
			if (isEnabled &&
			    !(checkBoxBlockSameStartTime.Checked || checkBoxBlockSameShift.Checked ||
			      checkBoxBlockSameShiftCategory.Checked))
				checkBoxBlockSameShiftCategory.Checked = true;
		}

		private void checkBoxBlockSameShiftCategory_CheckedChanged(object sender, EventArgs e)
		{
			teamBlockSchedulingSessionOptionsHandler();
		}

		private void checkBoxBlockSameStartTime_CheckedChanged(object sender, EventArgs e)
		{
			teamBlockSchedulingSessionOptionsHandler();
		}

		private void checkBoxBlockSameShift_CheckedChanged(object sender, EventArgs e)
		{
			teamBlockSchedulingSessionOptionsHandler();
		}

		private void teamBlockSchedulingSessionOptionsHandler()
		{
			if (!checkBoxBlockSameShift.Checked &&
			    !checkBoxBlockSameShiftCategory.Checked &&
			    !checkBoxBlockSameStartTime.Checked)
				comboBoxBlockType.SelectedIndex = 0;
		}

		private void checkBoxTeamSameShiftCategory_CheckedChanged(object sender, EventArgs e)
		{
			teamSchedulingSessionOptionsHandler();
		}

		private void checkBoxTeamSameStartTime_CheckedChanged(object sender, EventArgs e)
		{
			teamSchedulingSessionOptionsHandler();
		}

		private void checkBoxTeamSameEndTime_CheckedChanged(object sender, EventArgs e)
		{
			teamSchedulingSessionOptionsHandler();
		}

		private void checkBoxCommonActivity_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxTeamActivity.Enabled = checkBoxTeamSameActivity.Checked;
			teamSchedulingSessionOptionsHandler();
		}

		private void teamSchedulingSessionOptionsHandler()
		{
			if (!checkBoxTeamSameEndTime.Checked &&
				!checkBoxTeamSameShiftCategory.Checked &&
				!checkBoxTeamSameStartTime.Checked &&
				!checkBoxTeamSameActivity.Checked)
				comboBoxTeamGroupPage.SelectedIndex = 0;
		}
	}
}
