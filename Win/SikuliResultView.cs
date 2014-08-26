﻿using System.Drawing;
using System.Windows.Forms;
using Rhino.ServiceBus.DataStructures;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win
{
	public partial class SikuliResultView : Form
	{

		public SikuliResultView()
		{
			InitializeComponent();
		}

		public string Header
		{
			get { return labelTestInfoHeader.Text; }
			set
			{
				labelTestInfoHeader.Text = value;
				labelResult.Text = string.Empty;
			}
		}

		public bool Result
		{
			set
			{
				if (value)
				{
					labelResult.Text = "PASS";
					labelResult.ForeColor = System.Drawing.Color.Green;
				}
				else
				{
					labelResult.Text = "FAIL";
					labelResult.ForeColor = System.Drawing.Color.Red;
				}
				
			}
		}

		public string Details
		{
			set
			{
				textBoxDetails.Text = value;
			}
		}
	}
}
