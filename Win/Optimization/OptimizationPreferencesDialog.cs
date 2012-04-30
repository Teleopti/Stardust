using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Win.Optimization
{
    public partial class OptimizationPreferencesDialog : BaseRibbonForm, IDataExchange
    {

        public IOptimizationPreferences Preferences { get; private set; }
        private IList<IDataExchange> panels { get; set; }

        private readonly IList<IGroupPage> _groupPages;
        private readonly IList<IScheduleTag> _scheduleTags;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public OptimizationPreferencesDialog(
            IOptimizationPreferences preferences, 
            IList<IGroupPage> groupPages, 
            IList<IScheduleTag> scheduleTags)
            : this()
        {
            Preferences = preferences;
            _groupPages = groupPages;
            _scheduleTags = scheduleTags;
        }

        private OptimizationPreferencesDialog()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            generalPreferencesPanel1.Initialize(Preferences.General, _scheduleTags);
            dayOffPreferencesPanel1.Initialize(Preferences.DaysOff);
            extraPreferencesPanel1.Initialize(Preferences.Extra, _groupPages);
            advancedPreferencesPanel1.Initialize(Preferences.Advanced);
            panels = new List<IDataExchange>{generalPreferencesPanel1, dayOffPreferencesPanel1, extraPreferencesPanel1, advancedPreferencesPanel1};

            AddToHelpContext();
            SetColor();
        }

        #region IDataExchange Members

        public bool ValidateData(ExchangeDataOption direction)
        {
            return panels.All(panel => panel.ValidateData(direction));
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            panels.ToList().ForEach(panel => panel.ExchangeData(direction));
        }

        #endregion

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
            
            generalPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            dayOffPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            extraPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
            advancedPreferencesPanel1.BackColor = ColorHelper.DialogBackColor();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (ValidateData(ExchangeDataOption.ControlsToDataSource))
            {
                ExchangeData(ExchangeDataOption.ControlsToDataSource);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
