using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel
{
	public partial class ShiftCategoryDistributionControl : BaseUserControl, INeedShiftCategoryDistributionModel
	{
		public ShiftCategoryDistributionControl()
		{
			InitializeComponent();
			if(!DesignMode)
				SetTexts();
		}

		public IShiftCategoryDistributionModel Model { get; private set; }

		public void EnableOrDisableViewShiftCategoryDistribution(bool enable)
		{
			if (Model == null)
				return;
			Model.ShouldUpdateViews = enable;
		}

		public void SetModel(IShiftCategoryDistributionModel model)
		{
			Model = model;
			Model.ShouldUpdateViews = true;
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
