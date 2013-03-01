using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
	public partial class LevellingPerPrefrences : Form
	{
	    private LevellingPerConfiguration _levellingPerConfiguration;
	    private readonly IList<IGroupPageLight> _groupPagesLevelingPer;

	    public LevellingPerPrefrences(LevellingPerConfiguration levellingPerConfiguration, IList<IGroupPageLight> groupPagesLevelingPer)
		{
		    _levellingPerConfiguration = levellingPerConfiguration;
	        _groupPagesLevelingPer = groupPagesLevelingPer;
	        InitializeComponent();
		    InitializeValues();
		}

        

	    private void InitializeValues()
	    {
            comboBoxTeams.DataSource = _groupPagesLevelingPer;
            comboBoxTeams.DisplayMember = "Name";
            comboBoxTeams.ValueMember = "Key";
            comboBoxTeams.SelectedValue = "SingleAgentTeam";
            if (_levellingPerConfiguration._selectedGroupPage  != null)
                comboBoxTeams.SelectedValue = _levellingPerConfiguration._selectedGroupPage.Key ;

            switch(_levellingPerConfiguration._selectedBlockFinderType )
            {
                case BlockFinderType.SingleDay :
                    radioButtonSingleDay.Checked = true;
                    break;
                case BlockFinderType.Weeks :
                    radioButtonWeek.Checked = true;
                    break;
                case BlockFinderType.BetweenDayOff :
                    radioButtonBetweenDaysOff.Checked = true;
                    break;
                case BlockFinderType.SchedulePeriod :
                    radioButtonSchedulePeriod.Checked = true;
                    break;
            }

	        radioButtonSameShifts.Checked = _levellingPerConfiguration._UserSameShift;
	        radioButtonSame.Checked = _levellingPerConfiguration._UseSameEndTime ||
	                                  _levellingPerConfiguration._UseSameStartTime ||
	                                  _levellingPerConfiguration._UseSameShiftCategory;

	        checkBoxShiftCategory.Checked  = _levellingPerConfiguration._UseSameShiftCategory;
	        checkBoxEndTime.Checked = _levellingPerConfiguration._UseSameEndTime;
	        checkBoxStartTime.Checked = _levellingPerConfiguration._UseSameStartTime;
	    }


        //private void Form2_Load(object sender, EventArgs e)
        //{
        //    comboBoxTeams.SelectedIndex = 0;
        //}

		private void radioButton5_CheckedChanged(object sender, EventArgs e)
		{
			panelSame.Enabled = radioButtonSame.Checked;
		}

		private void button1_Click(object sender, EventArgs e)
		{
		    var levelling = new LevellingPerConfiguration();
            //block type
            if (radioButtonSingleDay.Checked)
                levelling._selectedBlockFinderType = BlockFinderType.SingleDay;
            else if (radioButtonWeek.Checked)
                levelling._selectedBlockFinderType = BlockFinderType.Weeks;
            else if (radioButtonBetweenDaysOff.Checked)
                levelling._selectedBlockFinderType = BlockFinderType.BetweenDayOff;
            else if (radioButtonSchedulePeriod.Checked)
                levelling._selectedBlockFinderType = BlockFinderType.SchedulePeriod;

            //selected page
            if ((string) comboBoxTeams.SelectedValue != "SingleAgentTeam")
                levelling._selectedGroupPage = (IGroupPageLight)comboBoxTeams.SelectedItem;
            else
                levelling._selectedGroupPage = null;

		    _levellingPerConfiguration = levelling;

		    _levellingPerConfiguration._UseSameEndTime = checkBoxEndTime.Checked;
		    _levellingPerConfiguration._UseSameShiftCategory = checkBoxShiftCategory.Checked;
		    _levellingPerConfiguration._UseSameStartTime = checkBoxStartTime.Checked;
		    _levellingPerConfiguration._UserSameShift = radioButtonSameShifts.Checked;

            Close();

		}

	    public LevellingPerConfiguration UpdatedLevellingPerConfiguration
	    {
            get { return _levellingPerConfiguration; }
	    }

        public LevellingPerConfiguration LevellingConfiguration
	    {
	        get { return _levellingPerConfiguration ; }
            set { _levellingPerConfiguration = value; }
	    }

	    private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			groupBoxUse.Enabled = !radioButtonSingleDay.Checked;
		}
	}

    public class LevellingPerConfiguration
    {
        public IGroupPageLight _selectedGroupPage;

        public BlockFinderType _selectedBlockFinderType;

        public bool _UserSameShift;

        public bool _UseSameStartTime;

        public bool _UseSameEndTime;

        public bool _UseSameShiftCategory;

    }
}
