using System;
using System.Windows.Forms;
using SdkTestClientWin.Domain;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
{
	public partial class SetWorkTimeOnPeriod : Form
	{
		private readonly Agent _agent;
		private readonly Form1 _form1;

		public SetWorkTimeOnPeriod()
		{
			InitializeComponent();
		}

		public SetWorkTimeOnPeriod(Agent agent, Form1 form1):this()
		{
			_agent = agent;
			_form1 = form1;
			label1.Text = agent.Dto.Name;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var val = (double)(numericUpDown1.Value * 60) ;
			var date = monthCalendar1.SelectionStart;
			var command = new SetSchedulePeriodWorktimeOverrideCommandDto
				{
					Date = new DateOnlyDto{DateTime = date},
					PeriodTimeInMinutes = val,
					PersonId = _agent.Dto.Id,
					PeriodTimeInMinutesSpecified = true
				};
			command.Date.DateTimeSpecified = true;
			var res = _form1.Service.OrganizationService.SetSchedulePeriodWorktimeOverride(command);
			if (res.AffectedId != null)
			{
				labelResult.Text = "Schedule Period " + res.AffectedId + " updated";
			}
			else
			{
				labelResult.Text = "Nothing updated!";
			}
		}

	}
}
