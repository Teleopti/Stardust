using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
    public partial class ReportScenarioSelector : BaseUserControl
    {
        private IScenario _selectedScenario;

        public ReportScenarioSelector()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public IScenario SelectedItem
        {
            get
            {
                return comboBoxAdvScenario.SelectedItem as Scenario;
            }
            set
            {
                _selectedScenario = value;
                setSelectedScenario();
            }
        }

        private void setSelectedScenario()
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
                var scenarioRepository = ScenarioRepository.DONT_USE_CTOR(unitOfWork);
                var scenarios = scenarioRepository.LoadAll().ToList();

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

                setSelectedScenario();
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
