using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Intraday
{
    public class IntradayNavigator : SchedulerNavigator
    {
        private readonly IIntradayViewFactory _intradayViewFactory;
        private readonly IScenarioProvider _scenarioProvider;

        private IOpenPeriodMode _intraday;

        public IntradayNavigator()
        {
            SelectorPresenter.ShowPersons = true;
        }

        public IntradayNavigator(IComponentContext container, PortalSettings portalSettings, IIntradayViewFactory intradayViewFactory, IScenarioProvider scenarioProvider)
            : base(container, portalSettings)
        {
            _intradayViewFactory = intradayViewFactory;
            _scenarioProvider = scenarioProvider;
            SetTexts();
            SetOpenToolStripText(Resources.Open);
            TodayButton.Visible = true;
            TodayButton.Click += toolStripButtonTodayClick;
        }

        private void toolStripButtonTodayClick(object sender, EventArgs e)
        {
            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IScenario scenario = _scenarioProvider.DefaultScenario();
                    var persons = new PersonRepository(uow).FindPeople(SelectorPresenter.SelectedPersonGuids);
                    var entityCollection = new Collection<IEntity>();
                    entityCollection.AddRange(persons.Cast<IEntity>());

                    StartModule(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), scenario, true, true, true, false, entityCollection);
                }
            }
            catch (DataSourceException dataSourceException)
            {
                ShowDataSourceException(dataSourceException, Resources.Open);
            }
            
        }
        
        protected override IApplicationFunction MyApplicationFunction
        {
            get
            {
                return ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                DefinedRaptorApplicationFunctionPaths.OpenIntradayPage);
            }
        }

        protected override IOpenPeriodMode OpenPeriodMode
        {
            get { return _intraday ?? (_intraday = new OpenPeriodIntradayMode()); }
        }

        protected override void StartModule(DateOnlyPeriod selectedPeriod, IScenario scenario, bool shrinkage, bool calculation, bool validation, bool teamLeaderMode, Collection<IEntity> entityCollection)
        {
            var intradayView = _intradayViewFactory.Create(selectedPeriod, scenario, entityCollection);
            ((Control)intradayView).Show();
        }

        
    }

    public interface IIntradayViewFactory
    {
        IIntradayView Create(DateOnlyPeriod dateOnlyPeriod, IScenario scenario, IEnumerable<IEntity> selectedEntities);
    }
}
