using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	public partial class ExportControl : BaseUserControl
	{
		public ExportControl()
		{
			InitializeComponent();
		}

		public ExportControl(IScenario scenario):this()
		{
			Tag = scenario;
			labelSaveTo.Text = Resources.SaveToSpace + scenario.Description.Name;
			SetTexts();
		}

		private void everyMouseLeave(object sender, EventArgs e)
		{
			BackColor = Color.White;
			ForeColor = Color.FromArgb(64, 64, 64);
		}

		private void everyMouseDown(object sender, MouseEventArgs e)
		{
			BackColor = Color.FromArgb(0, 153, 255);
			ForeColor = Color.White;
		}

		private void everyMouseEnter(object sender, EventArgs e)
		{
			BackColor = Color.FromArgb(128, 191, 234);
		}

		private void everyMouseUp(object sender, MouseEventArgs e)
		{
			BackColor = Color.FromArgb(128, 191, 234);
			ForeColor = Color.FromArgb(64, 64, 64);
		}

		private void everyMouseClick(object sender, EventArgs e)
		{
			OnClick(e);
		}

	}
}
