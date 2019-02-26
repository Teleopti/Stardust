using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class OvertimePreferencesDialog : BaseDialogForm, IOvertimePreferencesDialog
	{
		private OvertimePreferencesGeneralPersonalSetting _defaultOvertimeGeneralSettings;
		private readonly IOvertimePreferences _overtimePreferences;
		private readonly string _settingValue;
		private IEnumerable<IActivity> _availableActivity;
		private readonly int _resolution;
		private readonly IList<IMultiplicatorDefinitionSet> _definitionSets;
		private readonly IList<IRuleSetBag> _shiftBags;
		private readonly IEnumerable<IScheduleTag> _scheduleTags;
		private readonly OvertimePreferencesDialogPresenter _presenter;
		

		public OvertimePreferencesDialog(IEnumerable<IScheduleTag> scheduleTags, 
										string settingValue, 
										IEnumerable<IActivity> availableActivity, 
										int resolution, 
										IList<IMultiplicatorDefinitionSet> definitionSets,
										IList<IRuleSetBag> shiftBags,
										bool showUseSkills,
										bool useRightToLeft)
			: this(useRightToLeft)
		{
			_scheduleTags = scheduleTags;
			_settingValue = settingValue;
			_availableActivity = availableActivity;
			_resolution = resolution;
			_definitionSets = definitionSets;
			_shiftBags = shiftBags;
			_overtimePreferences = new OvertimePreferences();	
			_presenter = new OvertimePreferencesDialogPresenter(this);
			
			loadPersonalSettings();
			initTags();
			initActivityList();
			initOvertimeTypes();
			initShiftBags();
			initUseSkills(showUseSkills);
			setDefaultTimePeriod();
			setDefaultSpecificPeriod();
			setInitialValues();
			
			_presenter.SetStateButtons(_definitionSets);
		}

		public IOvertimePreferences Preferences
		{
			get { return _overtimePreferences; }
		}

		private void initUseSkills(bool show)
		{
			labelUseSkills.Visible = show;
			radioButtonAll.Visible = show;
			radioButtonPrimary.Visible = show;

			if (show)
			{
				radioButtonAll.Checked = _overtimePreferences.UseSkills.Equals(UseSkills.All);
				radioButtonPrimary.Checked = _overtimePreferences.UseSkills.Equals(UseSkills.Primary);
			}

			else
			{
				radioButtonAll.Checked = true;
				radioButtonPrimary.Checked = false;
			}
		}

		private void initOvertimeTypes()
		{
			comboBoxAdvOvertimeType.DataSource = _definitionSets;
			comboBoxAdvOvertimeType.DisplayMember = "Name";
			comboBoxAdvOvertimeType.SelectedItem = _overtimePreferences.OvertimeType;
		}

		private void initShiftBags()
		{
			var noneRuleSetBag = new RuleSetBag {Description = new Description(UserTexts.Resources.None)};
			_shiftBags.Insert(0, noneRuleSetBag);
			comboBoxShiftBags.DataSource = _shiftBags;
			comboBoxShiftBags.DisplayMember = "Description";
			if (_overtimePreferences.ShiftBagToUse == null)
				comboBoxShiftBags.SelectedIndex = 0;
			else
				comboBoxShiftBags.SelectedItem = _overtimePreferences.ShiftBagToUse;
		}

		private void overtimePreferencesDialogLoad(object sender, EventArgs e)
		{
			setDataToControls();
		}

		private void setDataToControls()
		{
			checkBoxAllowBreakingMaxTimePerWeek.Checked  = _overtimePreferences.AllowBreakMaxWorkPerWeek;
			checkBoxAllowBreakingNightlyRest.Checked  = _overtimePreferences.AllowBreakNightlyRest;
			checkBoxAllowBreakingWeeklyRest.Checked = _overtimePreferences.AllowBreakWeeklyRest;
			checkBoxOnAvailableAgentsOnly.Checked = _overtimePreferences.AvailableAgentsOnly;
		}

		private void initTags()
		{
			comboBoxAdvTag .DataSource = _scheduleTags;
			comboBoxAdvTag.DisplayMember = "Description";
			comboBoxAdvTag.SelectedItem = _overtimePreferences.ScheduleTag;
		}

		private void setDefaultTimePeriod()
		{
			fromToTimeDurationPicker1.StartTime.DefaultResolution = _resolution;
			fromToTimeDurationPicker1.EndTime.DefaultResolution = _resolution;

			fromToTimeDurationPicker1.StartTime.TimeIntervalInDropDown = _resolution;
			fromToTimeDurationPicker1.EndTime.TimeIntervalInDropDown = _resolution;

			TimeSpan start = TimeSpan.Zero;
			TimeSpan end = start.Add(TimeSpan.FromDays(1));
			
			fromToTimeDurationPicker1.StartTime.CreateAndBindList(start, end);
			fromToTimeDurationPicker1.EndTime.CreateAndBindList(start, end);

			fromToTimeDurationPicker1.StartTime.SetTimeValue(_overtimePreferences.SelectedTimePeriod.StartTime);
			fromToTimeDurationPicker1.EndTime.SetTimeValue(_overtimePreferences.SelectedTimePeriod.EndTime);

			fromToTimeDurationPicker1.StartTime.TextChanged += startTimeTextChanged;
			fromToTimeDurationPicker1.EndTime.TextChanged += endTimeTextChanged;
		}

		private void setDefaultSpecificPeriod()
		{

			fromToTimePickerSpecifiedPeriod.StartTime.DefaultResolution = _resolution;
			fromToTimePickerSpecifiedPeriod.EndTime.DefaultResolution = _resolution;

			fromToTimePickerSpecifiedPeriod.StartTime.TimeIntervalInDropDown = _resolution;
			fromToTimePickerSpecifiedPeriod.EndTime.TimeIntervalInDropDown = _resolution;

			var start = _overtimePreferences.SelectedSpecificTimePeriod.StartTime;
			var end = _overtimePreferences.SelectedSpecificTimePeriod.EndTime;

			fromToTimePickerSpecifiedPeriod.StartTime.CreateAndBindList(start, end);
			fromToTimePickerSpecifiedPeriod.EndTime.CreateAndBindList(start, end);

			fromToTimePickerSpecifiedPeriod.StartTime.SetTimeValue(start);
			fromToTimePickerSpecifiedPeriod.EndTime.SetTimeValue(end);

			fromToTimePickerSpecifiedPeriod.StartTime.TextChanged += startTimeTextChangedSpecific;
			fromToTimePickerSpecifiedPeriod.EndTime.TextChanged += endTimeTextChangedSpecific;
		}

		private void endTimeTextChanged(object sender, EventArgs e)
		{
			var startTime = fromToTimeDurationPicker1.StartTime.TimeValue();
			var endTime = fromToTimeDurationPicker1.EndTime.TimeValue();
			if (startTime > endTime)
				fromToTimeDurationPicker1.StartTime.SetTimeValue(endTime);
		}

		private void startTimeTextChanged(object sender, EventArgs e)
		{
			var startTime = fromToTimeDurationPicker1.StartTime.TimeValue();
			var endTime = fromToTimeDurationPicker1.EndTime.TimeValue();
			if (startTime > endTime)
				fromToTimeDurationPicker1.EndTime.SetTimeValue(startTime);
		}

		private void startTimeTextChangedSpecific(object sender, EventArgs e)
		{
			var startTime = fromToTimePickerSpecifiedPeriod.StartTime.TimeValue();
			var endTime = fromToTimePickerSpecifiedPeriod.EndTime.TimeValue();
			if (startTime > endTime)
				fromToTimePickerSpecifiedPeriod.EndTime.SetTimeValue(startTime);
		}

		private void endTimeTextChangedSpecific(object sender, EventArgs e)
		{
			var startTime = fromToTimePickerSpecifiedPeriod.StartTime.TimeValue();
			var endTime = fromToTimePickerSpecifiedPeriod.EndTime.TimeValue();
			if (startTime > endTime)
				fromToTimePickerSpecifiedPeriod.StartTime.SetTimeValue(endTime);
		}

		private void setInitialValues()
		{

			fromToTimeDurationPicker1.WholeDay.Visible = false;
			fromToTimePickerSpecifiedPeriod.WholeDay.Visible = false;

		}

		private void initActivityList()
		{
			_availableActivity = _availableActivity.Where(activity => activity.RequiresSkill).ToList();
			comboBoxAdvActivity.DataSource = _availableActivity;
			comboBoxAdvActivity.DisplayMember = "Name";
			comboBoxAdvActivity.ValueMember = "Name";

			if (_availableActivity.Contains(_overtimePreferences.SkillActivity))
				comboBoxAdvActivity.SelectedItem = _overtimePreferences.SkillActivity;
			else
				comboBoxAdvActivity.SelectedIndex = 0;
			
		}

		public OvertimePreferencesDialog(bool useRightToLeft)
		{
			InitializeComponent();
			if (!useRightToLeft)
			{
				if (!DesignMode) SetTextsNoRightToLeft();
			}
			else
			{
				if (!DesignMode) SetTexts();
			}
		}

		public void SavePersonalSettings()
		{
			if (hasMissedloadingSettings()) return;
			_defaultOvertimeGeneralSettings.MapFrom(_overtimePreferences);
			
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "GeneralSettings", _defaultOvertimeGeneralSettings);
					uow.PersistAll();

				}
			}
			catch (DataSourceException)
			{
			}
		}

		private void loadPersonalSettings()
		{
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var settingRepository = new PersonalSettingDataRepository(uow);
					_defaultOvertimeGeneralSettings = settingRepository.FindValueByKey(_settingValue + "GeneralSettings", new OvertimePreferencesGeneralPersonalSetting());
				}
			}
			catch (DataSourceException)
			{
			}
			if (hasMissedloadingSettings()) return;
			_defaultOvertimeGeneralSettings.MapTo(_overtimePreferences, _scheduleTags, _availableActivity,_definitionSets, _shiftBags );
		}

		private bool hasMissedloadingSettings()
		{
			return _defaultOvertimeGeneralSettings == null;
		}

		private void buttonOkClick(object sender, EventArgs e)
		{
			_presenter.ButtonOkClick();
			Close();
		}

		public void GetDataFromControls()
		{
			if (comboBoxAdvTag.SelectedText != "<None>")
			{
				_overtimePreferences.ScheduleTag = (IScheduleTag)comboBoxAdvTag.SelectedItem;
				
			}
			_overtimePreferences.SkillActivity = (IActivity)comboBoxAdvActivity.SelectedItem;
			_overtimePreferences.AllowBreakMaxWorkPerWeek = checkBoxAllowBreakingMaxTimePerWeek.Checked;
			_overtimePreferences.AllowBreakNightlyRest = checkBoxAllowBreakingNightlyRest.Checked ;
			_overtimePreferences.AllowBreakWeeklyRest = checkBoxAllowBreakingWeeklyRest.Checked ;
			_overtimePreferences.OvertimeType = (IMultiplicatorDefinitionSet) comboBoxAdvOvertimeType.SelectedItem;
			var selectedPeriod =new TimePeriod(fromToTimeDurationPicker1.StartTime.TimeValue(), fromToTimeDurationPicker1.EndTime.TimeValue());
			_overtimePreferences.SelectedTimePeriod = selectedPeriod;

			var selectedSpecificPeriod = new TimePeriod(fromToTimePickerSpecifiedPeriod.StartTime.TimeValue(), fromToTimePickerSpecifiedPeriod.EndTime.TimeValue());
			_overtimePreferences.SelectedSpecificTimePeriod = selectedSpecificPeriod;

			_overtimePreferences.AvailableAgentsOnly = checkBoxOnAvailableAgentsOnly.Checked;

			if (comboBoxShiftBags.Visible && comboBoxShiftBags.SelectedIndex != 0)
			{
				_overtimePreferences.ShiftBagToUse = (IRuleSetBag) comboBoxShiftBags.SelectedItem;
			}
			else
			{
				_overtimePreferences.ShiftBagToUse = null;
			}

			if (radioButtonAll.Checked)
			{
				_overtimePreferences.UseSkills = UseSkills.All;
			}
			else
			{
				_overtimePreferences.UseSkills = UseSkills.Primary;
			}
		}

		public void SetStateOkButtonDisabled()
		{
			buttonOK.Enabled = false;
			label4.BackColor = Color.Red;
		}
	}
}	
