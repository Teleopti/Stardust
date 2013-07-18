using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Win.Reporting
{
    public partial class ReportScenarioSelector : BaseUserControl
    {
        private IScenario _selectedScenario;

        public ReportScenarioSelector()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-27
        /// </remarks>
        public IScenario SelectedItem
        {
            get
            {
                return comboBoxAdvScenario.SelectedItem as Scenario;
            }
            set
            {
                _selectedScenario = value;
                SetSelectedScenario();
            }
        }

        private void SetSelectedScenario()
        {
            comboBoxAdvScenario.SelectedItem = _selectedScenario;
        }

        protected override void OnLoad(EventArgs e)
        {
            //if (DesignMode) return; //To make the control work in design mode!
            if (DesignMode || !StateHolderReader.IsInitialized) return;
            base.OnLoad(e);

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ScenarioRepository scenarioRepository = new ScenarioRepository(unitOfWork);
                IList<IScenario> scenarios = scenarioRepository.LoadAll();

                comboBoxAdvScenario.DisplayMember = "Description";
                comboBoxAdvScenario.DataSource = scenarios;

                if (_selectedScenario == null)
                {
                    _selectedScenario = scenarios[0];

                    foreach (IScenario scenario in scenarios)
                    {
                        if (scenario.DefaultScenario == true)
                        {
                            _selectedScenario = scenario;
                            break;
                        }
                    }
                }

                SetSelectedScenario();
            }
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }
    }
}
