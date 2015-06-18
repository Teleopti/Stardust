﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class AddCampaign : Form
	{
		private readonly IList<IActivity> _existingActivities;
		private Campaign _createdCampaign;
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

		public Campaign CreatedCampaign
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
			campaign.SpanningPeriod = new DateOnlyPeriod(new DateOnly(monthCalendar1.SelectionStart), new DateOnly(monthCalendar1.SelectionEnd));
			campaign.CallListLen = (int)numericUpDown1.Value;
			campaign.ConnectAverageHandlingTime = (int) numericUpDown2.Value;
			var campaignWorkingPeriod = new CampaignWorkingPeriod();
			campaignWorkingPeriod.TimePeriod = new TimePeriod(7, 0, 20, 0);

			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Monday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Tuesday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Wednesday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Thursday });
			campaignWorkingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Friday });

			campaign.AddWorkingPeriod(campaignWorkingPeriod);
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
