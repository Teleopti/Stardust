using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public partial class SelectTargetDatesAndScenario : BaseUserControl, IPropertyPageNoRoot<QuickForecastModel>
    {
		private QuickForecastModel _stateObj;
        private readonly ICollection<string> _errorMessages = new List<string>();
        private IList<IScenario> _scenarios;

        public SelectTargetDatesAndScenario()
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Populate(QuickForecastModel stateObj)
        {
            _stateObj = stateObj;
			reportDateFromToSelector1.WorkPeriodStart = new DateOnly(_stateObj.TargetPeriod.StartDate.DateTime);
			reportDateFromToSelector1.WorkPeriodEnd = new DateOnly(_stateObj.TargetPeriod.EndDate.DateTime);
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            loadScenarios();

            if (!noScenarioAvailable(_scenarios))
            {
                bindScenarioCombo(_scenarios);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastModel stateObj)
        {
			stateObj.TargetPeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = reportDateFromToSelector1.WorkPeriodStart },
				EndDate = new DateOnlyDto { DateTime = reportDateFromToSelector1.WorkPeriodEnd }
			};
			stateObj.ScenarioId = ((IScenario)comboBoxScenario.SelectedItem).Id.GetValueOrDefault();
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
            comboBoxScenario.DataSource = scenariosToLoad;
        }
       
    }
}
