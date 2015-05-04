using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class AddCampaign : Form
	{
		private Campaign _createdCampaign;

		public AddCampaign()
		{
			InitializeComponent();
		}

		public Campaign CreatedCampaign
		{
			get { return _createdCampaign; }
		}

		private void button1_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			var campaign = new Campaign();
			campaign.Name = textBox1.Text;
			campaign.StartDate = new DateOnly(monthCalendar1.SelectionStart);
			campaign.EndDate = new DateOnly(monthCalendar1.SelectionEnd);
			campaign.CallListLen = (int)numericUpDown1.Value;
			campaign.ConnectAverageHandlingTime = (int) numericUpDown2.Value;
			var campaignWorkingPeriod = new CampaignWorkingPeriod();
			campaignWorkingPeriod.TimePeriod = new TimePeriod(5, 0, 18, 0);

			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Monday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Tuesday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Wednesday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Thursday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Friday });

			campaign.AddWorkingPeriod(campaignWorkingPeriod);
			_createdCampaign = campaign;
			Close();
		}
	}
}
