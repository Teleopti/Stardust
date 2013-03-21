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
            if (_levellingPerConfiguration.SelectedGroupPage  != null)
                comboBoxTeams.SelectedValue = _levellingPerConfiguration.SelectedGroupPage.Key ;

            switch(_levellingPerConfiguration.SelectedBlockFinderType )
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

	        radioButtonSameShifts.Checked = _levellingPerConfiguration.UserSameShift;
            //radioButtonSame.Checked = _levellingPerConfiguration.UseSameEndTime ||
            //                          _levellingPerConfiguration.UseSameStartTime ||
            //                          _levellingPerConfiguration.UseSameShiftCategory;
	        if (_levellingPerConfiguration.UserSameShift)
                radioButtonSameShifts.Checked = true;
	        else
	        {
	            radioButtonSame.Checked = true;
                checkBoxShiftCategory.Checked = _levellingPerConfiguration.UseSameShiftCategory;
                checkBoxEndTime.Checked = _levellingPerConfiguration.UseSameEndTime;
                checkBoxStartTime.Checked = _levellingPerConfiguration.UseSameStartTime;
	        }
	        
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
                levelling.SelectedBlockFinderType = BlockFinderType.SingleDay;
            else if (radioButtonWeek.Checked)
                levelling.SelectedBlockFinderType = BlockFinderType.Weeks;
            else if (radioButtonBetweenDaysOff.Checked)
                levelling.SelectedBlockFinderType = BlockFinderType.BetweenDayOff;
            else if (radioButtonSchedulePeriod.Checked)
                levelling.SelectedBlockFinderType = BlockFinderType.SchedulePeriod;
            else
                levelling.SelectedBlockFinderType = BlockFinderType.None;
            //selected page
           levelling.SelectedGroupPage = (IGroupPageLight)comboBoxTeams.SelectedItem;



		    _levellingPerConfiguration = levelling;

		    _levellingPerConfiguration.UseSameEndTime = checkBoxEndTime.Checked;
		    _levellingPerConfiguration.UseSameShiftCategory = checkBoxShiftCategory.Checked;
		    _levellingPerConfiguration.UseSameStartTime = checkBoxStartTime.Checked;
		    _levellingPerConfiguration.UserSameShift = radioButtonSameShifts.Checked;

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
        public IGroupPageLight SelectedGroupPage { get; set; }

        public BlockFinderType SelectedBlockFinderType { get; set; }

        public bool UserSameShift { get; set; }

        public bool UseSameStartTime { get; set; }

        public bool UseSameEndTime;

        public bool UseSameShiftCategory;

         

    }
}
