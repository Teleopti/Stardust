﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;
using System.Windows.Forms;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class SchedulingSessionPreferencesDialog : BaseRibbonForm
    {

        private readonly ISchedulingOptions _schedulingOptions;
    	private readonly IDaysOffPreferences _daysOffPreferences;
    	private readonly IList<IShiftCategory> _shiftCategories;
        private readonly bool _backToLegal;
    	private readonly ISchedulerGroupPagesProvider _groupPagesProvider;
    	private readonly IList<IGroupPageLight> _groupPages;
        private readonly IList<IGroupPageLight> _groupPagesForTeamBlockPer;
        private readonly IList<IScheduleTag> _scheduleTags;
        private readonly string _settingValue;
        private readonly IList<IActivity> _availableActivity;
        private SchedulingOptionsGeneralPersonalSetting _defaultGeneralSettings;
		private SchedulingOptionsAdvancedPersonalSetting _defaultAdvancedSettings;
        private SchedulingOptionsExtraPersonalSetting _defaultExtraSettings;
	    private SchedulingOptionsDayOffPlannerPersonalSettings _defaultDayOffPlannerSettings;
    	

        private readonly bool _reschedule;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public SchedulingSessionPreferencesDialog(ISchedulingOptions schedulingOptions, IDaysOffPreferences daysOffPreferences, IList<IShiftCategory> shiftCategories,
            bool reschedule, bool backToLegal, ISchedulerGroupPagesProvider groupPagesProvider,
            IList<IScheduleTag> scheduleTags, string settingValue, IList<IActivity> availableActivity)
            : this()
        {
            _schedulingOptions = schedulingOptions;
        	_daysOffPreferences = daysOffPreferences;
        	_shiftCategories = shiftCategories;
            _reschedule = reschedule;
            
            _backToLegal = backToLegal;
        	_groupPagesProvider = groupPagesProvider;
        	_groupPages = groupPagesProvider.GetGroups(true);
            _groupPagesForTeamBlockPer = groupPagesProvider.GetGroups(true);
            //add the single agent 
            var singleAgentEntry = new GroupPageLight { Key = "SingleAgentTeam", Name = Resources.SingleAgentTeam };
            _groupPagesForTeamBlockPer.Add(singleAgentEntry);
            _scheduleTags = scheduleTags;
            _settingValue = settingValue;
		    _availableActivity = availableActivity;
        }

        private SchedulingSessionPreferencesDialog()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public ISchedulingOptions CurrentOptions
        {
            get { return _schedulingOptions; }
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
					if (_backToLegal)
						_defaultDayOffPlannerSettings = settingRepository.FindValueByKey(_settingValue + "DayOffPlannerSettings", new SchedulingOptionsDayOffPlannerPersonalSettings());
				}
			}
			catch (DataSourceException)
			{
			}
            if (hasMissedloadingSettings()) return;
			_defaultGeneralSettings.MapTo(_schedulingOptions, _scheduleTags);
			_defaultAdvancedSettings.MapTo(_schedulingOptions, _shiftCategories);
            _defaultExtraSettings.MapTo(_schedulingOptions, _scheduleTags, _groupPages,_groupPagesForTeamBlockPer, _availableActivity);
			if (_backToLegal && _defaultDayOffPlannerSettings != null)
				_defaultDayOffPlannerSettings.MapTo(_daysOffPreferences);
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
			if (_backToLegal && _defaultDayOffPlannerSettings != null)
				_defaultDayOffPlannerSettings.MapFrom(_daysOffPreferences);

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
					if (_backToLegal && _defaultDayOffPlannerSettings != null)
					{
						new PersonalSettingDataRepository(uow).PersistSettingValue(_settingValue + "DayOffPlannerSettings", _defaultDayOffPlannerSettings);
						uow.PersistAll();
					}
				}
			}
			catch (DataSourceException)
			{
			}
		}

        private void Form_Load(object sender, EventArgs e)
        {
        	loadPersonalSettings();

			schedulingSessionPreferencesTabPanel1.Initialize(_schedulingOptions, _shiftCategories, _reschedule,
                _backToLegal, _groupPagesProvider, _scheduleTags, _availableActivity);
            dayOffPreferencesPanel1.KeepFreeWeekendsVisible = false;
            dayOffPreferencesPanel1.KeepFreeWeekendDaysVisible = false;
			dayOffPreferencesPanel1.Initialize(_daysOffPreferences);
            AddToHelpContext();
            SetColor();
            SetOnOff(dayOffPreferencesPanel1);
            // don not use for now in scheduling
            if (!_backToLegal)
                tabControlTopLevel.TabPages.Remove(tabPageDayOffPlanningOptions);
            schedulingSessionPreferencesTabPanel1.ShiftCategoryVisible = true;
            schedulingSessionPreferencesTabPanel1.ScheduleOnlyAvailableDaysVisible = true;
            schedulingSessionPreferencesTabPanel1.ScheduleOnlyPreferenceDaysVisible = true;
            schedulingSessionPreferencesTabPanel1.ScheduleOnlyRotationDaysVisible = true;
            if (_schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.HourlyStaff)
            {
                _schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.None; 

                //_schedulingOptions.UseBlockScheduling = BlockFinderType.None;
                _schedulingOptions.UseGroupScheduling = false;

                _schedulingOptions.UseGroupSchedulingCommonStart = false;
                _schedulingOptions.UseGroupSchedulingCommonEnd = false;
                _schedulingOptions.UseGroupSchedulingCommonCategory = false;
                schedulingSessionPreferencesTabPanel1.HideTeamAndBlockSchedulingOptions();
            }
        }

        private void AddToHelpContext()
        {
            for (int i = 0; i < tabControlTopLevel.TabPages.Count; i++)
            {
                AddControlHelpContext(tabControlTopLevel.TabPages[i]);
            }
        }

        private void SetColor()
        {
            BackColor = ColorHelper.DialogBackColor();
            dayOffPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            schedulingSessionPreferencesTabPanel1.BackColor = ColorHelper.DialogBackColor();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if(schedulingSessionPreferencesTabPanel1.ValidateTeamSchedulingOption() )
            {
                schedulingSessionPreferencesTabPanel1.ExchangeData(ExchangeDataOption.ControlsToDataSource);

                if (dayOffPreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer))
                {
                    dayOffPreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    savePersonalSettings();
                }

                Close();
            }
            else
            {
                MessageBox.Show(UserTexts.Resources.SelectAtleastOneSchedulingOption, UserTexts.Resources.SchedulingOptionMessageBox, MessageBoxButtons.OK);
                DialogResult = DialogResult.None;
            }
            
        }

        private void tabControlTopLevel_Click(object sender, EventArgs e)
        {
            ActiveControl = tabControlTopLevel.SelectedTab;
        }

        private void dayOffPreferencesPanel1_StatusChanged(object sender, EventArgs e)
        {
            var panel = (ResourceOptimizerDayOffPreferencesPanel) sender;
            SetOnOff(panel);
        }

        private void SetOnOff(ResourceOptimizerDayOffPreferencesPanel panel)
        {
            if(panel.StatusIsOn())
            {
                tabPageDayOffPlanningOptions.ImageKey = "on"; 
            }
            else
            {
                tabPageDayOffPlanningOptions.ImageKey = "off";
            }
        }
    }
}
