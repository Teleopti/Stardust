using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Drawing;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class TestAdditionalInfoView : BaseRibbonForm
	{

		public TestAdditionalInfoView()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
		}

		public void CloseForm()
		{
			Close();
		}
	}
}
