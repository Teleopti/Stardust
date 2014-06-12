using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using System.Linq;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class OvertimePreferencesDialog : BaseRibbonForm
	{
        private OvertimePreferencesGeneralPersonalSetting _defaultOvertimeGeneralSettings;
	    private readonly IOvertimePreferences _overtimePreferences;
        private readonly string _settingValue;
	    private IEnumerable<IActivity> _availableActivity;
	    private readonly int _resolution;
	    private readonly IList<IMultiplicatorDefinitionSet> _definitionSets;
	    private readonly IEnumerable<IScheduleTag> _scheduleTags;

        public OvertimePreferencesDialog(IEnumerable<IScheduleTag> scheduleTags, string settingValue, IEnumerable<IActivity> availableActivity, int resolution, IList<IMultiplicatorDefinitionSet> definitionSets)
            : this()
        {
            _scheduleTags = scheduleTags;
            _settingValue = settingValue;
            _availableActivity = availableActivity;
            _resolution = resolution;
            _definitionSets = definitionSets;
            _overtimePreferences = new OvertimePreferences();
            
            loadPersonalSettings();
            initTags();
            initActivityList();
            initOvertimeTypes();
            setDefaultTimePeriod();
            setInitialValues();
        }

		public IOvertimePreferences Preferences
		{
			get { return _overtimePreferences; }
		}

	    private void initOvertimeTypes()
	    {
            comboBoxAdvOvertimeType.DataSource = _definitionSets;
            comboBoxAdvOvertimeType.DisplayMember = "Name";
            comboBoxAdvOvertimeType.SelectedItem = _overtimePreferences.OvertimeType;
	    }

	    private void OvertimePreferencesDialog_Load(object sender, EventArgs e)
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

        private void setInitialValues()
        {

            fromToTimeDurationPicker1.WholeDay.Visible = false;

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

	    public OvertimePreferencesDialog()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
		}

        private void savePersonalSettings()
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
            _defaultOvertimeGeneralSettings.MapTo(_overtimePreferences, _scheduleTags, _availableActivity,_definitionSets );
        }

        private bool hasMissedloadingSettings()
        {
            return _defaultOvertimeGeneralSettings == null;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            getDataFromControls();
            savePersonalSettings();
            Close();
        }

	    private void getDataFromControls()
	    {
	        if (comboBoxAdvTag.SelectedText != "<None>")
	        {
                _overtimePreferences.ScheduleTag = (IScheduleTag)comboBoxAdvTag.SelectedItem;
	            
	        }
            _overtimePreferences.SkillActivity = (IActivity)comboBoxAdvActivity.SelectedItem;
            _overtimePreferences.AllowBreakMaxWorkPerWeek = checkBoxAllowBreakingMaxTimePerWeek.Checked;
            _overtimePreferences.AllowBreakNightlyRest = checkBoxAllowBreakingNightlyRest.Checked ;
            _overtimePreferences.AllowBreakWeeklyRest = checkBoxAllowBreakingWeeklyRest.Checked ;
            _overtimePreferences.ExtendExistingShift = true;
	        _overtimePreferences.OvertimeType = (IMultiplicatorDefinitionSet) comboBoxAdvOvertimeType.SelectedItem;
            var selectedPeriod =new TimePeriod(fromToTimeDurationPicker1.StartTime.TimeValue(), fromToTimeDurationPicker1.EndTime.TimeValue());
	        _overtimePreferences.SelectedTimePeriod = selectedPeriod;
	        _overtimePreferences.AvailableAgentsOnly = checkBoxOnAvailableAgentsOnly.Checked;
	    }
	}
}
