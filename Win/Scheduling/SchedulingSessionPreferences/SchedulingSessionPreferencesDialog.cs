using System;
using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class SchedulingSessionPreferencesDialog : BaseRibbonForm
    {

        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IList<IShiftCategory> _shiftCategories;
        private readonly IDayOffPlannerRules _dayOffPlannerRules;
        private readonly bool _backToLegal;
    	private readonly IList<IGroupPage> _groupPages;
        private readonly SchedulingScreenSettings _currentSchedulingScreenSettings;
        private readonly IList<IScheduleTag> _scheduleTags;

        private readonly bool _reschedule;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public SchedulingSessionPreferencesDialog(ISchedulingOptions schedulingOptions, IDayOffPlannerRules dayOffPlannerRules, IList<IShiftCategory> shiftCategories,
            bool reschedule, bool backToLegal, IList<IGroupPage> groupPages, SchedulingScreenSettings currentSchedulingScreenSettings,
            IList<IScheduleTag> scheduleTags)
            : this()
        {
            _schedulingOptions = schedulingOptions;
            _dayOffPlannerRules = dayOffPlannerRules;
            _shiftCategories = shiftCategories;
            _reschedule = reschedule;
            
            _backToLegal = backToLegal;
        	_groupPages = groupPages;
            _currentSchedulingScreenSettings = currentSchedulingScreenSettings;
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

        private void Form_Load(object sender, EventArgs e)
        {
            schedulingSessionPreferencesPanel1.Initialize(_schedulingOptions, _shiftCategories, _reschedule, _backToLegal,_groupPages, 
                _currentSchedulingScreenSettings, false, _scheduleTags);
            dayOffPreferencesPanel1.KeepFreeWeekendsVisible = false;
            dayOffPreferencesPanel1.KeepFreeWeekendDaysVisible = false;
            dayOffPreferencesPanel1.Initialize(_dayOffPlannerRules);
            AddToHelpContext();
            SetColor();
            SetOnOff(dayOffPreferencesPanel1);
            // don not use for now in scheduling
            if (!_backToLegal)
                tabControlTopLevel.TabPages.Remove(tabPageDayOffPlanningOptions);
            schedulingSessionPreferencesPanel1.ShiftCategoryVisible = true;
            schedulingSessionPreferencesPanel1.ScheduleOnlyAvailableDaysVisible = true;
            schedulingSessionPreferencesPanel1.ScheduleOnlyPreferenceDaysVisible = true;
            schedulingSessionPreferencesPanel1.ScheduleOnlyRotationDaysVisible = true;
            schedulingSessionPreferencesPanel1.UseSameDayOffsVisible = false;
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
            schedulingSessionPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            schedulingSessionPreferencesPanel1.ExchangeData(ExchangeDataOption.ControlsToDataSource);

            if(dayOffPreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer))
            {
                dayOffPreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                DialogResult = System.Windows.Forms.DialogResult.OK;
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
