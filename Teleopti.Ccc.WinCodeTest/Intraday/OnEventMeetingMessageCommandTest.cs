﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
	public class OnEventMeetingMessageCommandTest
	{
		private IIntradayView _view;
		private IScenario _scenario;
		private ISchedulingResultLoader _schedulingResultLoader;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IRepositoryFactory _repositoryFactory;
		private ISchedulerStateHolder _schedulerStateHolder;
		private OnEventMeetingMessageCommand target;
		private readonly DateOnlyPeriod _period = new DateOnlyPeriod(2008, 10, 20, 2008, 10, 20);
		private IPerson _person;
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
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone), new[] { _person }, new DisableDeletedFilter(new FixedCurrentUnitOfWork(_unitOfWork)));
			_schedulerStateHolder.SchedulingResultState.PersonsInOrganization = _schedulerStateHolder.AllPermittedPersons;
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_schedulingResultLoader.Stub(x => x.SchedulerState).Return(_schedulerStateHolder);

			target = new OnEventMeetingMessageCommand(_view, _schedulingResultLoader, _unitOfWorkFactory, _repositoryFactory);
		}

		[Test]
		public void ShouldDeleteMeetingFromDictionary()
		{
			var idFromBroker = Guid.NewGuid();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			target.Execute(new EventMessage { InterfaceType = typeof(IMeetingChangedEntity), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Delete, ReferenceObjectId = _person.Id.GetValueOrDefault() });

			scheduleDictionary.AssertWasCalled(x => x.DeleteMeetingFromBroker(idFromBroker));
		}

		[Test]
		public void ShouldUpdateMeetingInDictionary()
		{
			var idFromBroker = Guid.NewGuid();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			target.Execute(new EventMessage { InterfaceType = typeof(IMeetingChangedEntity), DomainObjectId = idFromBroker, DomainUpdateType = DomainUpdateType.Update, ReferenceObjectId = _person.Id.GetValueOrDefault() });

			scheduleDictionary.Stub(x => x.DeleteMeetingFromBroker(idFromBroker));
			scheduleDictionary.Stub(x => x.MeetingUpdateFromBroker(Arg<IMeetingRepository>.Is.Anything, Arg<Guid>.Is.Equal(idFromBroker)));
		}
	}
}