using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
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
		private MockRepository mocks;
		private IIntradayView _view;
		private IScenario _scenario;
		private ISchedulingResultLoader _schedulingResultLoader;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IRepositoryFactory _repositoryFactory;
		private ISchedulerStateHolder _schedulerStateHolder;
		private OnEventScheduleMessageCommand target;
		private readonly DateOnlyPeriod _period = new DateOnlyPeriod(2008, 10, 20, 2008, 10, 20);
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_view = mocks.StrictMock<IIntradayView>();
			_scenario = mocks.StrictMock<IScenario>();
			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());
			_schedulingResultLoader = mocks.DynamicMock<ISchedulingResultLoader>();
			_unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
			_repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period,TeleoptiPrincipal.Current.Regional.TimeZone), new[]{_person});
			_schedulerStateHolder.SchedulingResultState.PersonsInOrganization = _schedulerStateHolder.AllPermittedPersons;
			
	        target = new OnEventScheduleMessageCommand(_view,_schedulingResultLoader,_unitOfWorkFactory,_repositoryFactory);
		}

		[Test]
		public void VerifyOnEventScheduleDataMessageHandlerAbsence()
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

			target.Execute(new EventMessage { InterfaceType = typeof(IPersonAbsence), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Insert, ReferenceObjectId = _person.Id.GetValueOrDefault()});

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventScheduleDataMessageHandlerAssignment()
		{
			var idFromBroker = Guid.NewGuid();
			var unitOfWork = mocks.StrictMock<IUnitOfWork>();
			var scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
			var personAssignmentRepository = mocks.StrictMock<IPersonAssignmentRepository>();

			setupCommonStuffForUpdatesFromBroker(unitOfWork, scheduleDictionary);

			Expect.Call(_repositoryFactory.CreatePersonAssignmentRepository(unitOfWork)).Return(
				personAssignmentRepository);
			Expect.Call(scheduleDictionary.UpdateFromBroker(personAssignmentRepository, idFromBroker)).Return(null);

			CommonStateHolder commonStateHolder = new CommonStateHolder();
			Expect.Call(_schedulerStateHolder.CommonStateHolder).Return(commonStateHolder).Repeat.Twice();
			Expect.Call(() => unitOfWork.Reassociate(commonStateHolder.Activities));
			Expect.Call(() => unitOfWork.Reassociate(commonStateHolder.ShiftCategories));
			Expect.Call(_schedulingResultLoader.Contracts).Return(new List<IContract>());
			Expect.Call(_schedulingResultLoader.ContractSchedules).Return(new List<IContractSchedule>());
			Expect.Call(() => _view.ReloadScheduleDayInEditor(_person));

			mocks.ReplayAll();

			target.Execute(new EventMessage { InterfaceType = typeof(IPersonAssignment), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Insert, ReferenceObjectId = _person.Id.GetValueOrDefault()});

			mocks.VerifyAll();
		}

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
		public void ShouldDeleteMeetingFromDictionary()
		{
			var idFromBroker = Guid.NewGuid();
			var scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();

			_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder);
			Expect.Call(()=>scheduleDictionary.DeleteMeetingFromBroker(idFromBroker));
			Expect.Call(_view.DrawSkillGrid);

			mocks.ReplayAll();

			target.Execute(new EventMessage { InterfaceType = typeof(IMeetingChangedEntity), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Delete, ReferenceObjectId = _person.Id.GetValueOrDefault() });

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventScheduleDataMessageHandlerMeeting()
		{
			Guid idFromBroker = Guid.NewGuid();
			IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
			IScheduleDictionary scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
			IMeetingRepository meetingRepository = mocks.StrictMock<IMeetingRepository>();

			setupCommonStuffForUpdatesFromBroker(unitOfWork, scheduleDictionary);

			Expect.Call(_repositoryFactory.CreateMeetingRepository(unitOfWork)).Return(
				meetingRepository);
			Expect.Call(()=>scheduleDictionary.DeleteMeetingFromBroker(idFromBroker));
			Expect.Call(()=>scheduleDictionary.MeetingUpdateFromBroker(meetingRepository, idFromBroker));

			CommonStateHolder commonStateHolder = new CommonStateHolder();
			Expect.Call(_schedulerStateHolder.CommonStateHolder).Return(commonStateHolder).Repeat.Twice();
			Expect.Call(() => unitOfWork.Reassociate(commonStateHolder.Activities));
			Expect.Call(() => unitOfWork.Reassociate(commonStateHolder.ShiftCategories));
			Expect.Call(_schedulingResultLoader.Contracts).Return(new List<IContract>());
			Expect.Call(_schedulingResultLoader.ContractSchedules).Return(new List<IContractSchedule>());
			Expect.Call(() => _view.ReloadScheduleDayInEditor(_person));

			mocks.ReplayAll();

			target.Execute(new EventMessage { InterfaceType = typeof(IMeetingChangedEntity), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Update, ReferenceObjectId = _person.Id.GetValueOrDefault() });

			mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void setupCommonStuffForUpdatesFromBroker(IUnitOfWork unitOfWork, IScheduleDictionary scheduleDictionary)
		{
			IList<IPerson> personsInOrganisation = new List<IPerson>{_person};
			_schedulerStateHolder = mocks.StrictMock<ISchedulerStateHolder>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			Expect.Call(unitOfWork.Dispose);

			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();
			Expect.Call(_schedulerStateHolder.RequestedScenario).Return(_scenario).Repeat.AtLeastOnce();
			Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(
				new SchedulingResultStateHolder(personsInOrganisation, scheduleDictionary,
												new Dictionary<ISkill, IList<ISkillDay>>())).Repeat.AtLeastOnce();
			Expect.Call(_schedulerStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
			Expect.Call(_schedulingResultLoader.InitializeScheduleData);

			Expect.Call(() => unitOfWork.Reassociate(_scenario));
			Expect.Call(() => unitOfWork.Reassociate(personsInOrganisation));

			Expect.Call(() => unitOfWork.Reassociate(new IContract[] { }));
			Expect.Call(() => unitOfWork.Reassociate(new IContractSchedule[] { }));

			Expect.Call(_view.DrawSkillGrid);
		}

		[Test]
		public void VerifyOnEventScheduleDataMessageHandlerDayOff()
		{
			Guid idFromBroker = Guid.NewGuid();
			IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
			IScheduleDictionary scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
			IPersonDayOffRepository personDayOffRepository = mocks.StrictMock<IPersonDayOffRepository>();

			setupCommonStuffForUpdatesFromBroker(unitOfWork, scheduleDictionary);

			Expect.Call(_repositoryFactory.CreatePersonDayOffRepository(unitOfWork)).Return(
				personDayOffRepository);
			Expect.Call(scheduleDictionary.UpdateFromBroker(personDayOffRepository, idFromBroker)).Return(null);

			CommonStateHolder commonStateHolder = new CommonStateHolder();
			Expect.Call(_schedulerStateHolder.CommonStateHolder).Return(commonStateHolder);
			Expect.Call(() => unitOfWork.Reassociate(commonStateHolder.DayOffs));
			Expect.Call(_schedulingResultLoader.Contracts).Return(new List<IContract>());
			Expect.Call(_schedulingResultLoader.ContractSchedules).Return(new List<IContractSchedule>());
			Expect.Call(() => _view.ReloadScheduleDayInEditor(_person));

			mocks.ReplayAll();

			target.Execute(new EventMessage { InterfaceType = typeof(IPersonDayOff), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Insert, ReferenceObjectId = _person.Id.GetValueOrDefault() });

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
		}
	}
}