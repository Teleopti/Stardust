using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Backlog
{
	public partial class AddCampaign : Form
	{
		private readonly IList<IActivity> _existingActivities;
        private IOutboundCampaign _createdCampaign;
		private IActivity _existingActivity;

		public AddCampaign()
		{
			InitializeComponent();
		}

		public AddCampaign(IList<IActivity> existingActivities)
		{
			_existingActivities = new List<IActivity>(existingActivities);
			InitializeComponent();
		}

        public IOutboundCampaign CreatedCampaign
		{
			get { return _createdCampaign; }
		}

		public IActivity ExistingActivity
		{
			get { return _existingActivity; }
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
			campaign.SpanningPeriod = new DateTimePeriod(new DateTime(monthCalendar1.SelectionStart.Ticks, DateTimeKind.Utc), new DateTime(monthCalendar1.SelectionEnd.Ticks, DateTimeKind.Utc));
			campaign.CallListLen = (int)numericUpDown1.Value;
			campaign.ConnectAverageHandlingTime = (int) numericUpDown2.Value;
			campaign.WorkingHours.Add(DayOfWeek.Monday, new TimePeriod(7, 0, 20, 0));
			campaign.WorkingHours.Add(DayOfWeek.Tuesday, new TimePeriod(7, 0, 20, 0));
			campaign.WorkingHours.Add(DayOfWeek.Wednesday, new TimePeriod(7, 0, 20, 0));
			campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod(7, 0, 20, 0));
			campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod(7, 0, 20, 0));

			_createdCampaign = campaign;
			if (!(comboBox1.SelectedItem is nullActivity))
				_existingActivity = (IActivity) comboBox1.SelectedItem;
			Close();
		}

		private void AddCampaign_Load(object sender, EventArgs e)
		{
			_existingActivities.Insert(0, new nullActivity());
			comboBox1.DataSource = _existingActivities;
			comboBox1.DisplayMember = "Name";
		}

		private class nullActivity : Activity
		{
			public nullActivity()
				: base("New activity")
			{
				
			}
		}
	}
}
