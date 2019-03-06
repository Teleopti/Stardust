using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public partial class ScenarioSelector : BaseUserControl
    {
        private IScenario _selectedScenario;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioSelector"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-27
        /// </remarks>
        public ScenarioSelector()
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

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return; //To make the control work in design mode!

			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ScenarioRepository scenarioRepository = ScenarioRepository.DONT_USE_CTOR(unitOfWork);
				var scenarios = scenarioRepository.LoadAll().ToList();

				comboBoxAdvScenario.DisplayMember = "Description";
				comboBoxAdvScenario.DataSource = scenarios;

				if (_selectedScenario == null)
				{
					_selectedScenario = scenarios[0];
				}
				SetSelectedScenario();
			}
        }
    }
}
