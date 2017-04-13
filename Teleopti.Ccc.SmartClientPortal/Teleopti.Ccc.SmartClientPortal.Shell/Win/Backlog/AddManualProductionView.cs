using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Backlog
{
	public partial class AddManualProductionView : Form
	{
        private readonly IOutboundCampaign _campaign;
		private IDictionary<DateOnly, TimeSpan> _manualProductionPlanDays = new Dictionary<DateOnly, TimeSpan>(); 

		public AddManualProductionView()
		{
			InitializeComponent();
		}

        public AddManualProductionView(IOutboundCampaign campaign)
		{
			InitializeComponent();
			_campaign = campaign;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			numericUpDown2.Value = -1;
			_manualProductionPlanDays.Remove(new DateOnly(monthCalendar1.SelectionStart));
		}

		private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
		{
			numericUpDown2.Value = -1;
			var manualTime = _campaign.GetManualProductionPlan(new DateOnly(monthCalendar1.SelectionStart));
			if (manualTime.HasValue)
				numericUpDown2.Value = (decimal)manualTime.Value.TotalHours;
		}

		private void AddManualProductionView_Load(object sender, EventArgs e)
		{
			monthCalendar1.MinDate = _campaign.SpanningPeriod.StartDateTime;
			monthCalendar1.MaxDate = _campaign.SpanningPeriod.EndDateTime;
						
			foreach (var dateOnly in _campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection())
			{
				var manualTime = _campaign.GetManualProductionPlan(dateOnly);
				if(manualTime.HasValue)
					_manualProductionPlanDays.Add(dateOnly, manualTime.Value);
			}

			setBoldDates();
		}

		private void setBoldDates()
		{
			var manualDates = new DateTime[_manualProductionPlanDays.Count];
			var index = 0;
			foreach (var dateOnly in _manualProductionPlanDays.Keys)
			{
				manualDates[index] = dateOnly.Date;
				index++;
			}
			monthCalendar1.MonthlyBoldedDates = manualDates;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void numericUpDown2_ValueChanged(object sender, EventArgs e)
		{
			var date = new DateOnly(monthCalendar1.SelectionStart);
			if(numericUpDown2.Value == -1)
				_manualProductionPlanDays.Remove(date);
			else
			{
				if (_manualProductionPlanDays.ContainsKey(date))
					_manualProductionPlanDays[date] = TimeSpan.FromHours((double) numericUpDown2.Value);
				else
				{
					_manualProductionPlanDays.Add(date, TimeSpan.FromHours((double)numericUpDown2.Value));
				}
			}

			//setBoldDates();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			foreach (var dateOnly in _campaign.SpanningPeriod.ToDateOnlyPeriod(_campaign.Skill.TimeZone).DayCollection())
			{
				_campaign.ClearProductionPlan(dateOnly);
				if (_manualProductionPlanDays.ContainsKey(dateOnly))
					_campaign.SetManualProductionPlan(dateOnly, _manualProductionPlanDays[dateOnly]);
			}
			Close();
		}
	}
}
