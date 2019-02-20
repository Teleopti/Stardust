using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingSessionPreferences
{
	public partial class SchedulingSessionPreferencesDialog : BaseDialogForm
	{
		private readonly SchedulingOptions _schedulingOptions;
		private readonly IEnumerable<IShiftCategory> _shiftCategories;
		private readonly SchedulerGroupPagesProvider _groupPagesProvider;
		private readonly IList<GroupPageLight> _groupPages;
		private readonly IList<GroupPageLight> _groupPagesForTeamBlockPer;
		private readonly IEnumerable<IScheduleTag> _scheduleTags;
		private readonly string _settingValue;
		private readonly IEnumerable<IActivity> _availableActivity;
		private SchedulingOptionsGeneralPersonalSetting _defaultGeneralSettings;
		private SchedulingOptionsAdvancedPersonalSetting _defaultAdvancedSettings;
		private SchedulingOptionsExtraPersonalSetting _defaultExtraSettings;
		private bool _useRightToLeft;

		public SchedulingSessionPreferencesDialog(
			SchedulingOptions schedulingOptions, 
			IEnumerable<IShiftCategory> shiftCategories, 
			SchedulerGroupPagesProvider groupPagesProvider,
			IEnumerable<IScheduleTag> scheduleTags, 
			string settingValue, 
			IEnumerable<IActivity> availableActivity,
			bool useRightToLeft)
			: this(useRightToLeft)
		{
			_schedulingOptions = schedulingOptions;
			_shiftCategories = shiftCategories;
			_groupPagesProvider = groupPagesProvider;
			_groupPages = groupPagesProvider.GetGroups(true);
			_groupPagesForTeamBlockPer = groupPagesProvider.GetGroups(true);
			//add the single agent 
			var singleAgentEntry = GroupPageLight.SingleAgentGroup(Resources.SingleAgentTeam);
			_groupPagesForTeamBlockPer.Add(singleAgentEntry);
			_scheduleTags = addKeepOriginalScheduleTag(scheduleTags);
			_settingValue = settingValue;
			_availableActivity = availableActivity;
		}

		private IEnumerable<IScheduleTag> addKeepOriginalScheduleTag(IEnumerable<IScheduleTag> scheduleTags)
		{
			var list = scheduleTags.ToList();
			var keepOriginalScheduleTag = KeepOriginalScheduleTag.Instance;
			list.Insert(1, keepOriginalScheduleTag);
			return list;
		}

		private SchedulingSessionPreferencesDialog(bool useRightToLeft)
		{
			InitializeComponent();
			_useRightToLeft = useRightToLeft;
			if (!useRightToLeft)
			{
				if (!DesignMode) SetTextsNoRightToLeft();
			}
			else
			{
				if (!DesignMode) SetTexts();
			}
		}

		private void loadPersonalSettings()
		{
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var settingRepository = new PersonalSettingDataRepository(uow);
					_defaultGeneralSettings = settingRepository.FindValueByKey(_settingValue + "GeneralSettings", new SchedulingOptionsGeneralPersonalSetting());
					_defaultAdvancedSettings = settingRepository.FindValueByKey(_settingValue + "AdvancedSettings", new SchedulingOptionsAdvancedPersonalSetting());
					_defaultExtraSettings = settingRepository.FindValueByKey(_settingValue + "ExtraSetting", new SchedulingOptionsExtraPersonalSetting());
				}
			}
			catch (DataSourceException)
			{
			}
			if (hasMissedloadingSettings()) return;
			_defaultGeneralSettings.MapTo(_schedulingOptions, _scheduleTags);
			_defaultAdvancedSettings.MapTo(_schedulingOptions, _shiftCategories);
			_defaultExtraSettings.MapTo(_schedulingOptions, _scheduleTags, _groupPages,_groupPagesForTeamBlockPer, _availableActivity);
		}

		private bool hasMissedloadingSettings()
		{
			return _defaultGeneralSettings == null || _defaultAdvancedSettings == null || _defaultExtraSettings == null;
		}

		private void savePersonalSettings()
		{
			if (hasMissedloadingSettings()) return;
			_defaultGeneralSettings.MapFrom(_schedulingOptions);
			_defaultAdvancedSettings.MapFrom(_schedulingOptions);
			_defaultExtraSettings.MapFrom(_schedulingOptions );

			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "GeneralSettings",_defaultGeneralSettings);
					uow.PersistAll();
					new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "AdvancedSettings",_defaultAdvancedSettings);
					uow.PersistAll();
					new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "ExtraSetting",_defaultExtraSettings );
					uow.PersistAll();
				}
			}
			catch (DataSourceException)
			{
			}
		}

		private void Form_Load(object sender, EventArgs e)
		{
			loadPersonalSettings();
			schedulingSessionPreferencesTabPanel1.Initialize(_schedulingOptions, _shiftCategories,_groupPagesProvider, _scheduleTags, _availableActivity, _useRightToLeft);
			addToHelpContext();
			setColor();
			schedulingSessionPreferencesTabPanel1.ShiftCategoryVisible = true;
			schedulingSessionPreferencesTabPanel1.ScheduleOnlyAvailableDaysVisible = true;
			schedulingSessionPreferencesTabPanel1.ScheduleOnlyPreferenceDaysVisible = true;
			schedulingSessionPreferencesTabPanel1.ScheduleOnlyRotationDaysVisible = true;
			if (_schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.HourlyStaff)
			{
				_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay; 
				_schedulingOptions.UseTeam = false;
				_schedulingOptions.TeamSameStartTime = false;
				_schedulingOptions.TeamSameEndTime = false;
				_schedulingOptions.TeamSameShiftCategory = false;
				schedulingSessionPreferencesTabPanel1.HideTeamAndBlockSchedulingOptions();
			}

			ActiveControl = schedulingSessionPreferencesTabPanel1;
		}

		private void addToHelpContext()
		{
			for (int i = 0; i < tabControlTopLevel.TabPages.Count; i++)
			{
				AddControlHelpContext(tabControlTopLevel.TabPages[i]);
			}
		}

		private void setColor()
		{
			BackColor = ColorHelper.DialogBackColor();
			schedulingSessionPreferencesTabPanel1.BackColor = ColorHelper.DialogBackColor();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		private void buttonOK_Click(object sender, EventArgs e)
		{
			schedulingSessionPreferencesTabPanel1.ExchangeData(ExchangeDataOption.ControlsToDataSource);
			DialogResult = DialogResult.OK;
			savePersonalSettings();
			Close();	
		}

		private void tabControlTopLevel_Click(object sender, EventArgs e)
		{
			ActiveControl = tabControlTopLevel.SelectedTab;
		}

		private void schedulingSessionPreferencesTabPanel1_Load(object sender, EventArgs e)
		{

		}


	}
}
