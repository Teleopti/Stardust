using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class SchedulingSessionPreferencesDialog : BaseRibbonForm
    {

        private readonly ISchedulingOptions _schedulingOptions;
    	private readonly IDaysOffPreferences _daysOffPreferences;
    	private readonly IList<IShiftCategory> _shiftCategories;
        private readonly bool _backToLegal;
    	private readonly IList<IGroupPage> _groupPages;
        private readonly IList<IScheduleTag> _scheduleTags;
    	private SchedulingOptionsGeneralPersonalSetting _defaultGeneralSettings;
		private SchedulingOptionsAdvancedPersonalSetting _defaultAdvancedSettings;
        private SchedulingOptionsExtraPersonalSetting _defaultExtraSettings;
    	

        private readonly bool _reschedule;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public SchedulingSessionPreferencesDialog(ISchedulingOptions schedulingOptions, IDaysOffPreferences daysOffPreferences, IList<IShiftCategory> shiftCategories,
            bool reschedule, bool backToLegal, IList<IGroupPage> groupPages,
            IList<IScheduleTag> scheduleTags)
            : this()
        {
            _schedulingOptions = schedulingOptions;
        	_daysOffPreferences = daysOffPreferences;
        	_shiftCategories = shiftCategories;
            _reschedule = reschedule;
            
            _backToLegal = backToLegal;
        	_groupPages = groupPages;
            _scheduleTags = scheduleTags;
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
					_defaultGeneralSettings = settingRepository.FindValueByKey("SchedulingOptionsGeneralSettings", new SchedulingOptionsGeneralPersonalSetting());
					_defaultAdvancedSettings = settingRepository.FindValueByKey("SchedulingOptionsAdvancedSettings", new SchedulingOptionsAdvancedPersonalSetting());
                    _defaultExtraSettings = settingRepository.FindValueByKey("SchedulingOptionsExtraSetting", new SchedulingOptionsExtraPersonalSetting());
				}
			}
			catch (DataSourceException)
			{
			}

			_defaultGeneralSettings.MapTo(_schedulingOptions, _scheduleTags, _groupPages);
			_defaultAdvancedSettings.MapTo(_schedulingOptions, _shiftCategories, _groupPages);
            _defaultExtraSettings.MapTo(_schedulingOptions,_scheduleTags,_groupPages );
		}

		private void savePersonalSettings()
		{
			_defaultGeneralSettings.MapFrom(_schedulingOptions);
			_defaultAdvancedSettings.MapFrom(_schedulingOptions);
            _defaultExtraSettings.MapFrom(_schedulingOptions );

			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultGeneralSettings);
					uow.PersistAll();
					new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultAdvancedSettings);
					uow.PersistAll();
                    new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultExtraSettings );
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

            schedulingSessionPreferencesTabPanel1.Initialize(_schedulingOptions, _shiftCategories, _reschedule, 
				_backToLegal,_groupPages,_scheduleTags);
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
            schedulingSessionPreferencesTabPanel1.UseSameDayOffsVisible = false;
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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            schedulingSessionPreferencesTabPanel1.ExchangeData(ExchangeDataOption.ControlsToDataSource);

            if(dayOffPreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer))
            {
                dayOffPreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                DialogResult = System.Windows.Forms.DialogResult.OK;
            	savePersonalSettings();
            }

            Close();
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
