using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages
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
			reportDateFromToSelector1.WorkPeriodStart =exportModel.Period.StartDate;
			reportDateFromToSelector1.WorkPeriodEnd = exportModel.Period.EndDate;
		}

		public bool Depopulate(ExportSkillModel stateObj)
		{
			stateObj.ExportMultisiteSkillToSkillCommandModel.Period = new DateOnlyPeriod
				(
				reportDateFromToSelector1.WorkPeriodStart,
				reportDateFromToSelector1.WorkPeriodEnd
				);
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
