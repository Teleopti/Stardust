using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
	public partial class SelectDateRange : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
	{
		private ExportSkillModel _stateObj;
		private readonly ICollection<string> _errorMessages = new List<string>();

		public SelectDateRange()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
				setColors();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			label1.BackColor = ColorHelper.WizardPanelBackgroundColor();
		}

		public void Populate(ExportSkillModel stateObj)
		{
			_stateObj = stateObj;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);

			var exportModel = _stateObj.ExportMultisiteSkillToSkillCommandModel;
			reportDateFromToSelector1.WorkPeriodStart = new DateOnly(exportModel.Period.StartDate.DateTime);
			reportDateFromToSelector1.WorkPeriodEnd = new DateOnly(exportModel.Period.EndDate.DateTime);
		}

		public bool Depopulate(ExportSkillModel stateObj)
		{
			stateObj.ExportMultisiteSkillToSkillCommandModel.Period = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto {DateTime = reportDateFromToSelector1.WorkPeriodStart},
					EndDate = new DateOnlyDto {DateTime = reportDateFromToSelector1.WorkPeriodEnd}
				};
			return true;	
		}

		public void SetEditMode()
		{
		}

		public string PageName
		{
			get { return Resources.SelectDates; }
		}

		public ICollection<string> ErrorMessages
		{
			get { return _errorMessages; }
		}
	}
}
