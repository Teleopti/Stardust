﻿using System;
using System.Web;

namespace Teleopti.Analytics.Portal
{
	public partial class PmContainer : MatrixBasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ContainerControl.Attributes.Add("src", "PerformanceManager/Default.aspx");
		}
	}
}