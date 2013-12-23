using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.Main
{
	public partial class ActiveAgents : Form
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

		private void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode.Equals(Keys.A))
			{
				textBox1.SelectAll();
			}
		}

		private void ActiveAgents_Load(object sender, EventArgs e)
		{
			Text = Resources.ActiveAgents;
		}
		
	}
}
