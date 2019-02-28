using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages
{
	public partial class SelectDateAndScenario : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
	{
		private ExportSkillModel _stateObj;
		private readonly ICollection<string> _errorMessages = new List<string>();
		private IList<IScenario> _scenarios;

		public SelectDateAndScenario()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				setColors();
				reportDateFromToSelector1.PeriodChanged += reportDateFromToSelector1PeriodChanged;
			}
		}

		void reportDateFromToSelector1PeriodChanged(object sender, EventArgs e)
		{
			if (hasScenarioDataChanged(_scenarios))
				bindScenarioCombo(_scenarios);
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
		}

		public void Populate(ExportSkillModel stateObj)
		{
			_stateObj = stateObj;
		}
		
		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			
			var exportModel = _stateObj.ExportSkillToFileCommandModel;
			reportDateFromToSelector1.WorkPeriodStart = exportModel.Period.StartDate == new DateOnly() ? DateOnly.Today : exportModel.Period.StartDate;
			reportDateFromToSelector1.WorkPeriodEnd = exportModel.Period.EndDate == new DateOnly() ? DateOnly.Today.AddDays(7) : exportModel.Period.EndDate;

			loadScenarios();

			if (!noScenarioAvailable(_scenarios))
			{
				bindScenarioCombo(_scenarios);
			}
		}

		public bool Depopulate(ExportSkillModel stateObj)
		{
			stateObj.ExportSkillToFileCommandModel.Period = new DateOnlyPeriod(reportDateFromToSelector1.WorkPeriodStart, reportDateFromToSelector1.WorkPeriodEnd);
			stateObj.ExportSkillToFileCommandModel.Scenario = (IScenario)comboBoxScenario.SelectedItem;
			return true;
		}

		public void SetEditMode()
		{
		}

		public string PageName
		{
			get { return Resources.SelectDateAndScenario; }
		}

		public ICollection<string> ErrorMessages
		{
			get { return _errorMessages; }
		}
		
		private static bool noScenarioAvailable(IList<IScenario> scenarios)
		{
			return scenarios.Count == 0;
		}

		private void loadScenarios()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_scenarios = ScenarioRepository.DONT_USE_CTOR(uow).FindAllSorted();
			}

			if (noScenarioAvailable(_scenarios))
			{
				_scenarios = new List<IScenario>();
			}
		}

		private bool hasScenarioDataChanged(IList<IScenario> scenariosToLoad)
		{
			if (comboBoxScenario.DataSource == null)
				return true;
			return (comboBoxScenario.Items.Count != scenariosToLoad.Count);
		}

		private void bindScenarioCombo(IList<IScenario> scenariosToLoad)
		{
			comboBoxScenario.DataSource = null;
			comboBoxScenario.DisplayMember = "Description";
			comboBoxScenario.DataSource = scenariosToLoad;
		}
	}
}
