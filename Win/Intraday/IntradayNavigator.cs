﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Intraday
{
    public class IntradayNavigator : SchedulerNavigator
    {
        private readonly IIntradayViewFactory _intradayViewFactory;
        private readonly ICurrentScenario _scenarioRepos;
    	private readonly IPersonRepository _personRepository;
    	private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IGracefulDataSourceExceptionHandler _gracefulDataSourceExceptionHandler;

    	private IOpenPeriodMode _intraday;

        public IntradayNavigator()
        {
            SelectorPresenter.ShowPersons = true;
        }

		  public IntradayNavigator(IComponentContext container, PortalSettings portalSettings, IIntradayViewFactory intradayViewFactory, ICurrentScenario scenarioRepos, IPersonRepository personRepository, IUnitOfWorkFactory unitOfWorkFactory, IGracefulDataSourceExceptionHandler gracefulDataSourceExceptionHandler)
            : base(container, portalSettings, personRepository, unitOfWorkFactory, gracefulDataSourceExceptionHandler)
        {
            _intradayViewFactory = intradayViewFactory;
            _scenarioRepos = scenarioRepos;
        	_personRepository = personRepository;
        	_unitOfWorkFactory = unitOfWorkFactory;
        	_gracefulDataSourceExceptionHandler = gracefulDataSourceExceptionHandler;
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

        	                                                                             			StartModule(new DateOnlyPeriod(DateOnly.Today,DateOnly.Today), scenario,true, true, true, false,entityCollection, null);
        	                                                                             		}
        	                                                                             	});

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

        protected override void StartModule(DateOnlyPeriod selectedPeriod, IScenario scenario, bool shrinkage, bool calculation, bool validation, bool teamLeaderMode, Collection<IEntity> entityCollection, Form ownerWindow)
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
