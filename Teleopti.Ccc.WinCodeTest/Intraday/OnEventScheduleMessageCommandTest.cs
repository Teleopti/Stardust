using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class OnEventScheduleMessageCommandTest
	{
		private IIntradayView _view;
		private IScenario _scenario;
		private ISchedulingResultLoader _schedulingResultLoader;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IRepositoryFactory _repositoryFactory;
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
            _repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
            _scheduleRefresher = MockRepository.GenerateMock<IScheduleRefresher>();
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period,TeleoptiPrincipal.Current.Regional.TimeZone), new[]{_person});
			_schedulerStateHolder.SchedulingResultState.PersonsInOrganization = _schedulerStateHolder.AllPermittedPersons;
            
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            _schedulingResultLoader.Stub(x => x.SchedulerState).Return(_schedulerStateHolder);
	        
            target = new OnEventScheduleMessageCommand(_view,_schedulingResultLoader,_unitOfWorkFactory, _scheduleRefresher);
		}

		[Test]
		public void ShouldReloadScheduleDayInEditorOnRefresh()
		{
		    var refreshedEntity = MockRepository.GenerateMock<IPersistableScheduleData>();

		    refreshedEntity.Stub(x => x.Person).Return(_person);
		    _scheduleRefresher.Stub(
		        x =>
		        x.Refresh(_schedulerStateHolder.Schedules, new List<IEventMessage>(), new List<IEventMessage>(), new Collection<IPersistableScheduleData>(), new Collection<PersistConflictMessageState>()))
                              .IgnoreArguments().WhenCalled(x => ((ICollection<IPersistableScheduleData>)x.Arguments[3]).Add(refreshedEntity));

			target.Execute(new EventMessage { InterfaceType = typeof(IScheduleChangedEvent), DomainObjectId = _person.Id.GetValueOrDefault(), DomainUpdateType = DomainUpdateType.NotApplicable, ReferenceObjectId = _scenario.Id.GetValueOrDefault()});

            _view.AssertWasCalled(x => x.ReloadScheduleDayInEditor(_person));
		}
        /*

                [Test]
                public void ShouldDeleteAssignmentFromDictionary()
                {
                    var idFromBroker = Guid.NewGuid();
                    var scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
			
                    _schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

                    Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder);
                    Expect.Call(scheduleDictionary.DeleteFromBroker(idFromBroker)).Return(null);
                    Expect.Call(_view.DrawSkillGrid);

                    mocks.ReplayAll();

                    target.Execute(new EventMessage { InterfaceType = typeof(IPersonAssignment), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Delete, ReferenceObjectId = _person.Id.GetValueOrDefault()});

                    mocks.VerifyAll();
                }

        
                [Test]
                public void VerifyOnEventScheduleDataMessageHandler()
                {
                    Guid idFromBroker = Guid.NewGuid();
                    IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
                    IScheduleDictionary scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
                    IPersonAbsenceRepository personAbsenceRepository = mocks.StrictMock<IPersonAbsenceRepository>();

                    setupCommonStuffForUpdatesFromBroker(unitOfWork, scheduleDictionary);

                    Expect.Call(_repositoryFactory.CreatePersonAbsenceRepository(unitOfWork)).Return(
                        personAbsenceRepository);
                    Expect.Call(scheduleDictionary.UpdateFromBroker(personAbsenceRepository, idFromBroker)).Return(null);

                    CommonStateHolder commonStateHolder = new CommonStateHolder();
                    Expect.Call(_schedulerStateHolder.CommonStateHolder).Return(commonStateHolder);
                    Expect.Call(() => unitOfWork.Reassociate(commonStateHolder.Absences));
                    Expect.Call(_schedulingResultLoader.Contracts).Return(new List<IContract>());
                    Expect.Call(_schedulingResultLoader.ContractSchedules).Return(new List<IContractSchedule>());
                    Expect.Call(() => _view.ReloadScheduleDayInEditor(_person));

                    mocks.ReplayAll();

                    target.Execute(new EventMessage { InterfaceType = typeof(IPersonAbsence), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Insert, ReferenceObjectId = _person.Id.GetValueOrDefault() });

                    mocks.VerifyAll();
                }*/

    }
}