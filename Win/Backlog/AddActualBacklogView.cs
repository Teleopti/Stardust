using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class AddActualBacklogView : Form
	{
		private readonly Campaign _campaign;
		private IDictionary<DateOnly, TimeSpan> _actualBacklogDays = new Dictionary<DateOnly, TimeSpan>(); 

		public AddActualBacklogView()
		{
			InitializeComponent();
		}

		public AddActualBacklogView(Campaign camapign)
		{
			InitializeComponent();
			_campaign = camapign;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			numericUpDown2.Value = -1;
			_actualBacklogDays.Remove(new DateOnly(monthCalendar1.SelectionStart));
		}

		private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
		{
			numericUpDown2.Value = -1;
			var manualTime = _campaign.GetManualProductionPlan(new DateOnly(monthCalendar1.SelectionStart));
			if (manualTime.HasValue)
				numericUpDown2.Value = (decimal)manualTime.Value.TotalHours;
		}

		private void AddActualBacklogView_Load(object sender, EventArgs e)
		{
			monthCalendar1.MinDate = _campaign.SpanningPeriod.StartDate.Date;
			monthCalendar1.MaxDate = _campaign.SpanningPeriod.EndDate.Date;

			foreach (var dateOnly in _campaign.SpanningPeriod.DayCollection())
			{
				var manualTime = _campaign.GetActualBacklog(dateOnly);
				if (manualTime.HasValue)
					_actualBacklogDays.Add(dateOnly, manualTime.Value);
			}

			setBoldDates();
		}

		private void setBoldDates()
		{
			var manualDates = new DateTime[_actualBacklogDays.Count];
			var index = 0;
			foreach (var dateOnly in _actualBacklogDays.Keys)
			{
				manualDates[index] = dateOnly.Date;
				index++;
			}
			monthCalendar1.MonthlyBoldedDates = manualDates;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			foreach (var dateOnly in _campaign.SpanningPeriod.DayCollection())
			{
				_campaign.ClearActualBacklog(dateOnly);
				if (_actualBacklogDays.ContainsKey(dateOnly))
					_campaign.SetActualBacklog(dateOnly, _actualBacklogDays[dateOnly]);
			}
			Close();
		}
	}
}
