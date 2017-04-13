using System;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
	public partial class SelectExportType : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
	{
		private readonly Action<bool> _callbackOnPageChange;
		private readonly ICollection<string> _errorMessages = new List<string>();

		protected SelectExportType()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
				setColors();
		}

		public SelectExportType(Action<bool> callbackOnPageChange) : this()
		{
			_callbackOnPageChange = callbackOnPageChange;
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
		}

		public void Populate(ExportSkillModel stateObj)
		{
			rbtExportToBU.Visible = stateObj.DirectExportPermitted;
			rbtExportToFile.Visible = stateObj.FileExportPermitted;
			if (stateObj.ExportToFile)
			{
				rbtExportToFile.Checked = true;
			}
			else
			{
				rbtExportToBU.Checked = true;
			}
		}

		public bool Depopulate(ExportSkillModel stateObj)
		{
			_callbackOnPageChange(rbtExportToFile.Checked);
			stateObj.ChangeExportType(rbtExportToFile.Checked);
			return true;
		}

		public void SetEditMode()
		{
		}

		public string PageName
		{
			get { return Resources.FileTypeSelection; }
		}

		public ICollection<string> ErrorMessages
		{
			get { return _errorMessages; }
		}
	}
}
