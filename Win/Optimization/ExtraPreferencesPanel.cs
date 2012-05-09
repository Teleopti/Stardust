using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
    public partial class ExtraPreferencesPanel : BaseUserControl, IDataExchange
    {

        private IList<IGroupPage> _groupPageOnTeams;
        private IList<IGroupPage> _groupPageOnCompareWith;

        public IExtraPreferences Preferences { get; private set; }

        public ExtraPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void Initialize(
            IExtraPreferences extraPreferences,
            IList<IGroupPage> groupPages)
        {
            Preferences = extraPreferences;

            var specification = new NotSkillGroupSpecification();
            _groupPageOnTeams = new List<IGroupPage>(groupPages).FindAll(specification.IsSatisfiedBy);
            _groupPageOnCompareWith = new List<IGroupPage>(groupPages).FindAll(specification.IsSatisfiedBy);
            ExchangeData(ExchangeDataOption.DataSourceToControls);
            setInitialControlStatus();
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
                BindGroupPages();
                setDataToControls();
            }
            else
            {
                getDataFromControls();
            }
        }

        #endregion

        private void BindGroupPages()
        {
            comboBoxGroupPageOnTeams.DataSource = _groupPageOnTeams;
            comboBoxGroupPageOnTeams.DisplayMember = "Description";
            comboBoxGroupPageOnTeams.ValueMember = "Key";

            comboBoxGroupPageOnCompareWith.DataSource = _groupPageOnCompareWith;
            comboBoxGroupPageOnCompareWith.DisplayMember = "Description";
            comboBoxGroupPageOnCompareWith.ValueMember = "Key";
        }
    
        private void getDataFromControls()
        {
            Preferences.UseBlockScheduling = checkBoxBlock.Checked;

            Preferences.BlockFinderTypeValue = 
                radioButtonBetweenDayOff.Checked 
                ? BlockFinderType.BetweenDayOff 
                : BlockFinderType.SchedulePeriod;


            Preferences.UseTeams = checkBoxTeams.Checked;

            Preferences.GroupPageOnTeam = (IGroupPage)comboBoxGroupPageOnTeams.SelectedItem;

            Preferences.FairnessValue = (double)trackBar1.Value / 100;

            Preferences.GroupPageOnCompareWith = (IGroupPage)comboBoxGroupPageOnCompareWith.SelectedItem;

            Preferences.KeepShiftCategories = checkBoxKeepShiftCategories.Checked;
            Preferences.KeepStartAndEndTimes = checkBoxKeepStartEndTimes.Checked;
            Preferences.KeepShifts = checkBoxKeepShifts.Checked;
 
            Preferences.KeepShiftsValue = (double)numericUpDownKeepShifts.Value  / 100;
        }

        private void setDataToControls()
        {
            checkBoxBlock.Checked = Preferences.UseBlockScheduling;

            switch(Preferences.BlockFinderTypeValue)
            {
                case BlockFinderType.BetweenDayOff:
                    radioButtonBetweenDayOff.Checked = true;
                    break;
                case BlockFinderType.SchedulePeriod:
                    radioButtonSchedulePeriod.Checked = true;
                    break;
            }

            checkBoxTeams.Checked = Preferences.UseTeams;

            if (Preferences.GroupPageOnTeam != null)
                comboBoxGroupPageOnTeams.SelectedValue = Preferences.GroupPageOnTeam.Key;
            if (comboBoxGroupPageOnTeams.SelectedValue == null)
                comboBoxGroupPageOnTeams.SelectedIndex = 0;

            trackBar1.Value = (int)(Preferences.FairnessValue * 100);


            if (Preferences.GroupPageOnCompareWith != null)
                comboBoxGroupPageOnCompareWith.SelectedValue = Preferences.GroupPageOnCompareWith.Key;
            if (comboBoxGroupPageOnCompareWith.SelectedValue == null)
                comboBoxGroupPageOnCompareWith.SelectedIndex = 0;


            checkBoxKeepShiftCategories.Checked = Preferences.KeepShiftCategories;
            checkBoxKeepStartEndTimes.Checked = Preferences.KeepStartAndEndTimes;
            checkBoxKeepShifts.Checked = Preferences.KeepShifts;

            numericUpDownKeepShifts.Value = (decimal) Preferences.KeepShiftsValue * 100;
        }

        private void checkBoxBlock_CheckedChanged(object sender, System.EventArgs e)
        {
            setRadioButtonsStatus();
        }

        private void setRadioButtonsStatus()
        {
            radioButtonBetweenDayOff.Enabled = checkBoxBlock.Checked;
            radioButtonSchedulePeriod.Enabled = checkBoxBlock.Checked;
        }

        private void checkBoxTeams_CheckedChanged(object sender, System.EventArgs e)
        {
            setComboGroupPageOnTeamsStatus();
        }

        private void setComboGroupPageOnTeamsStatus()
        {
            comboBoxGroupPageOnTeams.Enabled = checkBoxTeams.Checked;
        }

        private void checkBoxKeepShifts_CheckedChanged(object sender, System.EventArgs e)
        {
            setNumericUpDownKeepShiftsStatus();
        }

        private void setNumericUpDownKeepShiftsStatus()
        {
            numericUpDownKeepShifts.Enabled = checkBoxKeepShifts.Checked;
        }

        private void setInitialControlStatus()
        {
            setRadioButtonsStatus();
            setComboGroupPageOnTeamsStatus();
            setNumericUpDownKeepShiftsStatus();
        }
    }
}
