using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	public partial class ShiftCategoryDistributionControl : BaseUserControl, INeedShiftCategoryDistributionModel
	{
		public ShiftCategoryDistributionControl()
		{
			InitializeComponent();
			if(!DesignMode)
				SetTexts();
		}

		public void SetModel(IShiftCategoryDistributionModel model)
		{
			foreach (var tabPage in tabControlShiftCategoryDistribution.TabPages)
			{
				var page = tabPage as TabPageAdv;
				if (page == null)
					continue;

				setModelRec(model, page);
			}
		}

		private void setModelRec(IShiftCategoryDistributionModel model, Control control)
		{
			foreach (var childControl in control.Controls)
			{
				var child = childControl as INeedShiftCategoryDistributionModel;
				if(child != null)
					child.SetModel(model);

				var child1 = childControl as Control;
				if (child1 != null)
					setModelRec(model, child1);
			}
		}
	}
}
