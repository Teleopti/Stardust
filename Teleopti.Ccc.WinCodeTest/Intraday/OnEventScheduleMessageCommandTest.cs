using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;

using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class OnEventScheduleMessageCommandTest
	{
		private IIntradayView _view;
		private IScenario _scenario;
		private ISchedulingResultLoader _schedulingResultLoader;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private ISchedulerStateHolder _schedulerStateHolder;
		private OnEventScheduleMessageCommand target;
		private readonly DateOnlyPeriod _period = new DateOnlyPeriod(2008, 10, 20, 2008, 10, 20);
		private IPerson _person;
	    private IScheduleRefresher _scheduleRefresher;
	    private IUnitOfWork _unitOfWork;

	    [SetUp]
		public void Setup()
		{
			_view = MockRepository.GenerateMock<IIntradayView>();
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());
            _schedulingResultLoader = MockRepository.GenerateMock<ISchedulingResultLoader>();
            _unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            _unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            MockRepository.GenerateMock<IRepositoryFactory>();
            _scheduleRefresher = MockRepository.GenerateMock<IScheduleRefresher>();
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), new[] { _person }, new DisableDeletedFilter(new ThisUnitOfWork(_unitOfWork)), new SchedulingResultStateHolder());
			_schedulerStateHolder.SchedulingResultState.LoadedAgents = _schedulerStateHolder.ChoosenAgents;
            
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            _schedulingResultLoader.Stub(x => x.SchedulerState).Return(_schedulerStateHolder);
	        
            target = new OnEventScheduleMessageCommand(_view,_schedulingResultLoader,_unitOfWorkFactory, _scheduleRefresher);
		}

		[Test]
		public void ShouldReloadScheduleDayInEditorOnRefresh()
		{
		    var refreshedEntity = MockRepository.GenerateMock<IPersistableScheduleData>();
			var eventMessage = new EventMessage { InterfaceType = typeof(IScheduleChangedEvent), DomainObjectId = _person.Id.GetValueOrDefault(), DomainUpdateType = DomainUpdateType.NotApplicable, ReferenceObjectId = _scenario.Id.GetValueOrDefault() };
			
		    refreshedEntity.Stub(x => x.Person).Return(_person);
			_scheduleRefresher.Stub(
				x =>
				x.Refresh(_schedulerStateHolder.Schedules, new List<IEventMessage>(), new List<IPersistableScheduleData>(),
				          new List<PersistConflict>(), _ => true))
			                  .Constraints(Is.Equal(_schedulerStateHolder.Schedules),
			                               Rhino.Mocks.Constraints.List.IsIn(eventMessage),
			                               Is.Anything(),
			                               Is.Anything(),
										   Is.Anything())
			                  .WhenCalled(x => ((ICollection<IPersistableScheduleData>) x.Arguments[2]).Add(refreshedEntity));

		    refreshedEntity.Stub(x => x.Person).Return(_person);
		   target.Execute(eventMessage);

            _view.AssertWasCalled(x => x.ReloadScheduleDayInEditor(_person));
		}

    }
}