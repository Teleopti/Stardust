using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class OvertimePreferencesDialog : BaseRibbonForm
	{
        private OvertimePreferencesGeneralPersonalSetting _defaultOvertimeGeneralSettings;
	    private IOvertimePreferences _overtimePreferences;
        private readonly string _settingValue;
	    private readonly IList<IActivity> _availableActivity;
	    private readonly int _resolution;
	    private readonly IList<IScheduleTag> _scheduleTags;

        public OvertimePreferencesDialog(IList<IScheduleTag> scheduleTags, string settingValue, IList<IActivity> availableActivity, int resolution)
            : this()
        {
            _scheduleTags = scheduleTags;
            _settingValue = settingValue;
            _availableActivity = availableActivity;
            _resolution = resolution;
            initTags();
            initActivityList();
            SetDefaultTimePeriod();
            SetInitialValues();
        }

        private void initTags()
        {
            comboBoxAdvTag .DataSource = _scheduleTags;
            comboBoxAdvTag.DisplayMember = "Description";
            comboBoxAdvTag.ValueMember = "Description";
            //if (_localSchedulingOptions.CommonActivity != null)
            //{
            //    comboBoxActivity.SelectedValue = _localSchedulingOptions.CommonActivity.Name;
            //}
        }

        private void SetDefaultTimePeriod()
        {
            fromToTimePicker1.StartTime.DefaultResolution = _resolution;
            fromToTimePicker1.EndTime.DefaultResolution = _resolution;

            fromToTimePicker1.StartTime.TimeIntervalInDropDown = _resolution;
            fromToTimePicker1.EndTime.TimeIntervalInDropDown = _resolution;

            TimeSpan start = TimeSpan.Zero;
            TimeSpan end = TimeSpan.Zero;

            fromToTimePicker1.StartTime.CreateAndBindList(start, end);
            fromToTimePicker1.EndTime.CreateAndBindList(start, end);

            fromToTimePicker1.StartTime.SetTimeValue(start);
            fromToTimePicker1.EndTime.SetTimeValue(end);

            fromToTimePicker1.StartTime.TextChanged += startTimeTextChanged;
            fromToTimePicker1.EndTime.TextChanged += endTimeTextChanged;
        }

        private void endTimeTextChanged(object sender, EventArgs e)
        {
            var startTime = fromToTimePicker1.StartTime.TimeValue();
            var endTime = fromToTimePicker1.EndTime.TimeValue();
            if (startTime > endTime)
                fromToTimePicker1.StartTime.SetTimeValue(endTime);
        }

        private void startTimeTextChanged(object sender, EventArgs e)
        {
            var startTime = fromToTimePicker1.StartTime.TimeValue();
            var endTime = fromToTimePicker1.EndTime.TimeValue();
            if (startTime > endTime)
                fromToTimePicker1.EndTime.SetTimeValue(startTime);
        }

        private void SetInitialValues()
        {

            fromToTimePicker1.WholeDay.Visible = false;

        }

        private void initActivityList()
        {
            comboBoxAdvActivity.DataSource = _availableActivity;
            comboBoxAdvActivity.DisplayMember = "Name";
            comboBoxAdvActivity.ValueMember = "Name";
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

        private bool hasMissedloadingSettings()
        {
            return _defaultOvertimeGeneralSettings == null;
        }

        private void OvertimePreferencesDialog_Load(object sender, EventArgs e)
        {
            loadPersonalSettings();
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
            _defaultOvertimeGeneralSettings.MapTo(_overtimePreferences,_scheduleTags,_availableActivity );
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            savePersonalSettings();
            Close();
        }

        
	}
}
