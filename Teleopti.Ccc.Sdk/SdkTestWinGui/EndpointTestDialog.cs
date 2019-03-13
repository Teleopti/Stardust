using SdkTestClientWin.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SdkTestWinGui
{
	public partial class EndpointTestDialog : Form
	{
		public Form1 MainForm { get; set; }
		public EndpointTestDialog(Form1 owner)
		{
			InitializeComponent();
			MainForm = owner;
		}

		private void btnDoAction_Click(object sender, EventArgs e)
		{
			var qo = new GetSchedulesByChangeDateQueryDto
			{
				ChangesFromUTC = new DateTime(2019, 02, 06, 12, 00, 00),
				ChangesFromUTCSpecified = true,
				Page = 0,
				PageSpecified = true

			};
			tbResponsOutput.Text += $"# Query: '{qo.ChangesFromUTC}' -> '{qo.ChangesToUTC}', Page: '{qo.Page}', PageSize: '{qo.PageSize}'";

			var resp = MainForm.Service.SchedulingService.GetSchedulesByChangedDateTime(qo);
			tbResponsOutput.Text += $"  - ChangesUpToUTC: '{resp.ChangesUpToUTC}' {resp.Page}  {resp.TotalPages} {resp.TotalSchedules}. Schedules: '{resp.Schedules.Count()}";
		}
	}
}
