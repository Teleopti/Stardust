using System;
using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class ResourceOptimizerPreferencesDialog : BaseRibbonForm
    {

        #region Variables

        private readonly IDayOffPlannerRules _dayOffPlannerRules;
        private readonly IOptimizerAdvancedPreferences _optimizerAdvancedPreferences;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IOptimizerOriginalPreferences _optimizerOriginalPreferences;
        private readonly IList<IShiftCategory> _shiftCategories;
    	private readonly IList<IGroupPage> _groupPages;
        private readonly SchedulingScreenSettings _currentSchedulingScreenSettings;
        private readonly IList<IScheduleTag> _scheduleTags;

        #endregion

        #region Constructors

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ResourceOptimizerPreferencesDialog(IOptimizerOriginalPreferences optimizerOriginalPreferences, IList<IShiftCategory> shiftCategories,
            IList<IGroupPage> groupPages, SchedulingScreenSettings currentSchedulingScreenSettings, IList<IScheduleTag> scheduleTags) 
            :this()
        {
            _optimizerOriginalPreferences = optimizerOriginalPreferences;
            _dayOffPlannerRules = _optimizerOriginalPreferences.DayOffPlannerRules;
            _optimizerAdvancedPreferences = _optimizerOriginalPreferences.AdvancedPreferences;
            _schedulingOptions = _optimizerOriginalPreferences.SchedulingOptions;
    	    
    	    _shiftCategories = shiftCategories;
    		_groupPages = groupPages;
    	    _currentSchedulingScreenSettings = currentSchedulingScreenSettings;
            _scheduleTags = scheduleTags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOptimizerPreferencesDialog"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-01-15
        /// </remarks>
        private ResourceOptimizerPreferencesDialog()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        #endregion

        #region Interface

        /// <summary>
        /// Gets the day off planner rules.
        /// </summary>
        /// <value>The day off planner rules.</value>
        public IDayOffPlannerRules DayOffPlannerRules
        {
            get { return _dayOffPlannerRules; }
        }

        /// <summary>
        /// Gets the resource re optimizer preferences.
        /// </summary>
        /// <value>The resource re optimizer preferences.</value>
        public IOptimizerAdvancedPreferences OptimizerAdvancedPreferences
        {
            get { return _optimizerAdvancedPreferences; }
        }

        /// <summary>
        /// Gets the scheduling options.
        /// </summary>
        /// <value>The scheduling options.</value>
        public ISchedulingOptions SchedulingOptions
        {
            get { return _schedulingOptions; }
        }

        #endregion

        #region Handle events

        private void Form_Load(object sender, EventArgs e)
        {
            dayOffPreferencesPanel1.Initialize(DayOffPlannerRules);
            resourceOptimizerUserPreferencesPanel1.Initialize(OptimizerAdvancedPreferences);
            resourceOptimizerPerformancePreferencesPanel1.Initialize(OptimizerAdvancedPreferences);
            schedulingSessionPreferencesPanel1.Initialize(SchedulingOptions, _shiftCategories, true, false, _groupPages,
                _currentSchedulingScreenSettings, true, _scheduleTags);
            schedulingSessionPreferencesPanel1.RefreshScreenVisible = true;
            //schedulingSessionPreferencesPanel1.UseBlockSchedulingVisible = false;
            //schedulingSessionPreferencesPanel1.BetweenDayOffVisible = false;
            //schedulingSessionPreferencesPanel1.SchedulePeriodVisible = false;
            AddToHelpContext();
            SetColor();
        }

        private void AddToHelpContext()
        {
            for (int i = 0; i < tabControlResourceReOptimizerOptions.TabPages.Count; i++)
            {
                AddControlHelpContext(tabControlResourceReOptimizerOptions.TabPages[i]);
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (dayOffPreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer)
                && resourceOptimizerUserPreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer)
                && resourceOptimizerPerformancePreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer)
                && schedulingSessionPreferencesPanel1.ValidateData(ExchangeDataOption.ClientToServer))
            {
                dayOffPreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                resourceOptimizerUserPreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                resourceOptimizerPerformancePreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                schedulingSessionPreferencesPanel1.ExchangeData(ExchangeDataOption.ClientToServer);
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        private void SetColor()
        {
            BackColor = ColorHelper.DialogBackColor();
            resourceOptimizerUserPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            dayOffPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            resourceOptimizerPerformancePreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            schedulingSessionPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            tabPageDayOffPlannerOptions.BackColor = ColorHelper.DialogBackColor();
            tabPageAdvancedOptions.BackColor = ColorHelper.DialogBackColor();
            tabSchedulingOptions.BackColor = ColorHelper.DialogBackColor();
        }
        private void tabControlResourceReOptimizerPerformanceOptions_Click(object sender, EventArgs e)
        {
            ActiveControl = tabControlResourceReOptimizerOptions.SelectedTab;
        }

    }
}