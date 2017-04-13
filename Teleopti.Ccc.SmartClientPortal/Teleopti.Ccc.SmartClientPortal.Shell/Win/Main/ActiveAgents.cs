using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Main
{
	public partial class ActiveAgents : BaseDialogForm
	{
		public ActiveAgents()
		{
			InitializeComponent();
		}

		public ActiveAgents(IEnumerable<string> strings):this()
		{
			foreach (var s in strings)
			{
				textBox1.AppendText(s + Environment.NewLine);
			}
		}

		private void textBox1KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode.Equals(Keys.A))
			{
				textBox1.SelectAll();
			}
		}

		private void activeAgentsLoad(object sender, EventArgs e)
		{
			Text = Resources.ActiveAgents;
		}
		
	}
}
