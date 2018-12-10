using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Main;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday
{
	public class IntradayNavigator : SchedulerNavigator
	{
		private readonly IIntradayViewFactory _intradayViewFactory;
		private readonly ICurrentScenario _scenarioRepos;
		private readonly IPersonRepository _personRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGracefulDataSourceExceptionHandler _gracefulDataSourceExceptionHandler;
		private readonly IApplicationInsights _applicationInsights;

		private IOpenPeriodMode _intraday;

		public IntradayNavigator(IApplicationInsights applicationInsights)
		{
			_applicationInsights = applicationInsights;
			SelectorPresenter.ShowPersons = true;
		}

		public IntradayNavigator(IComponentContext container, PortalSettings portalSettings,
			IIntradayViewFactory intradayViewFactory, ICurrentScenario scenarioRepos, IPersonRepository personRepository,
			IUnitOfWorkFactory unitOfWorkFactory, IGracefulDataSourceExceptionHandler gracefulDataSourceExceptionHandler, IApplicationInsights applicationInsights)
			: base(container, portalSettings, personRepository, unitOfWorkFactory, gracefulDataSourceExceptionHandler, applicationInsights)
		{
			_intradayViewFactory = intradayViewFactory;
			_scenarioRepos = scenarioRepos;
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_gracefulDataSourceExceptionHandler = gracefulDataSourceExceptionHandler;
			_applicationInsights = applicationInsights;
			SetTexts();
			SetOpenToolStripText(Resources.Open);
			TodayButton.Visible = true;
			TodayButton.Click += toolStripButtonTodayClick;
		}

		private void toolStripButtonTodayClick(object sender, EventArgs e)
		{
			_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
																													  {
																														  using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
																														  {
																															  IScenario scenario = _scenarioRepos.Current();
																															  var persons = _personRepository.FindPeople(SelectorPresenter.SelectedPersonGuids);
																															  var entityCollection = new Collection<IEntity>();
																															  entityCollection.AddRange(persons.Cast<IEntity>());

																															  StartModule(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), scenario, true, true, true, false, false, entityCollection, null);
																														  }
																													  });

		}

		protected override IApplicationFunction MyApplicationFunction
		{
			get
			{
				return ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.OpenIntradayPage);
			}
		}

		protected override IOpenPeriodMode OpenPeriodMode
		{
			get { return _intraday ?? (_intraday = new OpenPeriodIntradayMode()); }
		}

		protected override void StartModule(DateOnlyPeriod selectedPeriod, IScenario scenario, bool shrinkage, bool calculation, bool validation, bool teamLeaderMode, bool loadRequests, Collection<IEntity> entityCollection, Form ownerWindow)
		{
			var intradayView = _intradayViewFactory.Create(selectedPeriod, scenario, entityCollection);
			((Control)intradayView).Show();
			_applicationInsights.TrackEvent("Opened classic intraday for period in Intraday Module.");
		}


	}

	public interface IIntradayViewFactory
	{
		IIntradayView Create(DateOnlyPeriod dateOnlyPeriod, IScenario scenario, IEnumerable<IEntity> selectedEntities);
	}
}
