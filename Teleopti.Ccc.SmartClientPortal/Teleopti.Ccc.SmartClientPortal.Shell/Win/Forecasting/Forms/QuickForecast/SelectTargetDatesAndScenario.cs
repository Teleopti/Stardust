using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectTargetDatesAndScenario : BaseUserControl, IPropertyPageNoRoot<QuickForecastModel>
	{
		private QuickForecastModel _stateObj;
		private readonly ICollection<string> _errorMessages = new List<string>();
		private IList<IScenario> _scenarios;

		public SelectTargetDatesAndScenario()
		{
			InitializeComponent();
			if (DesignMode) return;
			SetTexts();
			setColors();
			TargetFromTo.PeriodChanged += reportDateFromToSelector1PeriodChanged;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Populate(QuickForecastModel stateObj)
		{
			_stateObj = stateObj;
			TargetFromTo.EnableNullDates = false;
			TargetFromTo.WorkPeriodStart = _stateObj.TargetPeriod.StartDate;
			TargetFromTo.WorkPeriodEnd = _stateObj.TargetPeriod.EndDate;
			checkBoxUseDayOfMonth.Checked = stateObj.UseDayOfMonth;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			loadScenarios();

			if (!noScenarioAvailable(_scenarios))
			{
				bindScenarioCombo(_scenarios);
			}
			TargetFromTo.WorkPeriodStart = _stateObj.TargetPeriod.StartDate;
			TargetFromTo.WorkPeriodEnd = _stateObj.TargetPeriod.EndDate;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastModel stateObj)
		{
			stateObj.TargetPeriod = new DateOnlyPeriod
			(
				TargetFromTo.WorkPeriodStart,
				TargetFromTo.WorkPeriodEnd
			);
			stateObj.ScenarioId = ((IScenario)comboBoxScenario.SelectedItem).Id.GetValueOrDefault();
			stateObj.UseDayOfMonth = checkBoxUseDayOfMonth.Checked;
			return true;
		}

		public void SetEditMode()
		{
		}

		public string PageName
		{
			get { return Resources.GenerateForecastFor; }
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
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_scenarios = new ScenarioRepository(uow).FindAllSorted();
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
			comboBoxScenario.ValueMember = "Id";
			comboBoxScenario.DataSource = scenariosToLoad;
		}

		public override string HelpId
		{
			get
			{
				return "Help";
			}
		}

	}
}
