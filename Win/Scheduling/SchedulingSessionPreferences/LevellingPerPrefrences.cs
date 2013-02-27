using System;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
	public partial class LevellingPerPrefrences : Form
	{
		public LevellingPerPrefrences()
		{
			InitializeComponent();
		}

		private void Form2_Load(object sender, EventArgs e)
		{
			comboBox1.SelectedIndex = 0;
		}

		private void radioButton5_CheckedChanged(object sender, EventArgs e)
		{
			panelSame.Enabled = radioButton5.Checked;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			groupBoxUse.Enabled = !radioButton1.Checked;
		}
	}
}
