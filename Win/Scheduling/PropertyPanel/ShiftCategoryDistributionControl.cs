using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	public partial class ShiftCategoryDistributionControl : BaseUserControl, INeedShiftCategoryDistributionModel
	{
		public ShiftCategoryDistributionControl()
		{
			InitializeComponent();
		}

		public void SetModel(IShiftCategoryDistributionModel model)
		{
			foreach (var tabPage in tabControlShiftCategoryDistribution.TabPages)
			{
				var page = tabPage as TabPageAdv;
				if (page == null)
					continue;

				foreach (var control in page.Controls)
				{
					var distributionControl = control as INeedShiftCategoryDistributionModel;
					if(distributionControl != null)
						distributionControl.SetModel(model);
				}

			}
		}
	}
}
