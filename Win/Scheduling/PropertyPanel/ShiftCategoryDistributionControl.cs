using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	public partial class ShiftCategoryDistributionControl : BaseUserControl, INeedShiftCategoryDistributionModel
	{
		private IShiftCategoryDistributionModel _model;

		public ShiftCategoryDistributionControl()
		{
			InitializeComponent();
			if(!DesignMode)
				SetTexts();
		}

		public void DisableViewShiftCategoryDistribution()
		{
			if(_model != null)
				_model.ShouldUpdateViews = false;
		}

		public void EnableViewShiftCategoryDistribution()
		{
			if (_model == null)
				return;
			_model.ShouldUpdateViews = true;
		}

		public void SetModel(IShiftCategoryDistributionModel model)
		{
			_model = model;
			_model.ShouldUpdateViews = true;
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
