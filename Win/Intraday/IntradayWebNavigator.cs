using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Win.Intraday
{
	public partial class IntradayWebNavigator : Common.BaseUserControl
	{
		private readonly IComponentContext _container;

		public IntradayWebNavigator(IComponentContext container)
		{
			_container = container;
			InitializeComponent();
			SetTexts();
		}

		private string buildWfmUri(string relativePath)
		{
			var wfmPath = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle");
			return new Uri($"{wfmPath}{relativePath}").ToString();
		}

		private void toolStripButtonIntraday_Click(object sender, EventArgs e)
		{
			Process.Start(buildWfmUri("WFM/#/intraday"));
		}

		private void toolStripButtonRta_Click(object sender, EventArgs e)
		{
			Process.Start(buildWfmUri("WFM/#/rta"));
		}

		private void toolStripButtonOldIntraday_Click(object sender, EventArgs e)
		{
			IntradayNavigator oldIntradayNavigator = _container.Resolve<IntradayNavigator>();
			oldIntradayNavigator.Dock = DockStyle.Fill;
			this.Parent.Controls.Add(oldIntradayNavigator);
			oldIntradayNavigator.BringToFront();
			((TableLayoutPanel)this.Parent.Parent).RowStyles[2].Height = 50F;
		}
	}
}