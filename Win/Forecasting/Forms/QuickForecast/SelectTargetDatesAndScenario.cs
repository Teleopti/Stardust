using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectTargetDatesAndScenario : BaseUserControl, IPropertyPageNoRoot<QuickForecastCommandDto>
    {
		private QuickForecastCommandDto _stateObj;
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
		public void Populate(QuickForecastCommandDto stateObj)
        {
            _stateObj = stateObj;
			TargetFromTo.EnableNullDates = false;
			TargetFromTo.WorkPeriodStart = new DateOnly(_stateObj.TargetPeriod.StartDate.DateTime);
			TargetFromTo.WorkPeriodEnd = new DateOnly(_stateObj.TargetPeriod.EndDate.DateTime);
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
			TargetFromTo.WorkPeriodStart = new DateOnly(_stateObj.TargetPeriod.StartDate.DateTime);
			TargetFromTo.WorkPeriodEnd = new DateOnly(_stateObj.TargetPeriod.EndDate.DateTime);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastCommandDto stateObj)
        {
			stateObj.TargetPeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = TargetFromTo.WorkPeriodStart },
				EndDate = new DateOnlyDto { DateTime = TargetFromTo.WorkPeriodEnd }
			};
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
