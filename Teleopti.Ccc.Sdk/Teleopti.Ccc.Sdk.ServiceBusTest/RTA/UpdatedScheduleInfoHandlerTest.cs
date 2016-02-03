using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class UpdatedScheduleInfoHandlerTest
	{
		private MockRepository mocks;
		private IDelayedMessageSender serviceBus;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private IPersonRepository personRepository;
		private PersonActivityChangePulseLoop target;
		private INotifyRtaToCheckForActivityChange teleoptiRtaService;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			serviceBus = mocks.StrictMock<IDelayedMessageSender>();
			scheduleProjectionReadOnlyRepository = mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();
			teleoptiRtaService = mocks.DynamicMock<INotifyRtaToCheckForActivityChange>();
			personRepository = MockRepository.GenerateStub<IPersonRepository>();
			target = new PersonActivityChangePulseLoop(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService, personRepository);
		}

		[Test]
		public void IsPersonActivityStartingConsumeSuccessfully()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

            var personInfoMessage = new PersonActivityChangePulseEvent
				{
					PersonId = person.Id.GetValueOrDefault(),
					Timestamp = period.StartDateTime,
					LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault(),
					LogOnDatasource = "DS"
				};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					  person.Id.GetValueOrDefault())).IgnoreArguments()
				  .Return(DateTime.UtcNow.AddHours(3));
			Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(personInfoMessage);
			mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void DoNotSendDelayMessageIfThereIsNoNextActivity()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					 person.Id.GetValueOrDefault())).IgnoreArguments()
				 .Return(null);
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void SendDelayMessageIfThereExistNextActivity()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					person.Id.GetValueOrDefault())).IgnoreArguments()
				.Return(DateTime.UtcNow.AddHours(3));
			Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void IsScheduleProjectionReadOnlyChangedConsumeSuccessfully()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
				{
					ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
					ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
					PersonId = person.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
				};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					 person.Id.GetValueOrDefault())).IgnoreArguments()
				 .Return(DateTime.UtcNow.AddHours(3));
			Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[Test]
		public void DoNotSendAnyMessageIfTheScheduleIsNotUpdatedWithinRange()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddDays(3),
				ActivityEndDateTime = DateTime.UtcNow.AddDays(4),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					  person.Id.GetValueOrDefault()))
				  .IgnoreArguments().Return(DateTime.UtcNow.AddDays(2));
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[Test]
		public void SendMessageIfTheScheduleIsUpdatedWithinRange()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					 person.Id.GetValueOrDefault())).IgnoreArguments()
				 .Return(DateTime.UtcNow.AddHours(3));
			Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_NoNextActivity_ShouldNotEnqueue()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
				{
					ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
					ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
					PersonId = person.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
				};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					  person.Id.GetValueOrDefault()))
				  .IgnoreArguments()
				  .Return(null);
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_NextActivityBeforeScheduleChange_ShouldNotEnqueue()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(4),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					  person.Id.GetValueOrDefault()))
				  .IgnoreArguments()
				  .Return(DateTime.UtcNow.AddHours(1));
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_NextActivityAfterScheduleChange_ShouldEnqueue()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					  person.Id.GetValueOrDefault()))
				  .IgnoreArguments()
				  .Return(DateTime.UtcNow.AddHours(4));
			Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow, null))
				  .IgnoreArguments();
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);


			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_PersonDoesNotHaveExternalLogOn_ShouldDiscard()
		{
			teleoptiRtaService = MockRepository.GenerateMock<INotifyRtaToCheckForActivityChange>();
			target = new PersonActivityChangePulseLoop(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService, personRepository);
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();

			var updatedScheuldeDay = new ScheduleProjectionReadOnlyChanged
				{
					ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
					ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
					PersonId = personId,
					LogOnBusinessUnitId = businessUnitId
				};
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(false);

			target.Handle(updatedScheuldeDay);
			
			teleoptiRtaService.AssertWasNotCalled(r => r.CheckForActivityChange(Guid.Empty, Guid.Empty, DateTime.UtcNow), a => a.IgnoreArguments());
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_PersonHaveExternalLogOn_ShouldSend()
		{
			teleoptiRtaService = MockRepository.GenerateMock<INotifyRtaToCheckForActivityChange>();
			target = new PersonActivityChangePulseLoop(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService, personRepository);
			var updatedScheuldeDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = Guid.Empty,
				LogOnBusinessUnitId = Guid.Empty
			};
			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty)).IgnoreArguments().Return(true);

			target.Handle(updatedScheuldeDay);

			teleoptiRtaService.AssertWasCalled(s => s.CheckForActivityChange(Guid.Empty, Guid.Empty, DateTime.UtcNow), a => a.IgnoreArguments());
		}

		[Test]
		public void PersonActivityStarting_PersonDoesNotHaveExternalLogOn_ShouldDiscard()
		{
			teleoptiRtaService = MockRepository.GenerateMock<INotifyRtaToCheckForActivityChange>();
			target = new PersonActivityChangePulseLoop(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService, personRepository);

			var personActivityStarting = new PersonActivityChangePulseEvent
				{
					PersonId = Guid.NewGuid(),
					LogOnBusinessUnitId = Guid.Empty,
					Timestamp = DateTime.UtcNow
				};

			personRepository.Stub(x => x.DoesPersonHaveExternalLogOn(DateOnly.Today, personActivityStarting.PersonId))
							.Return(false);

			target.Handle(personActivityStarting);
			
			teleoptiRtaService.AssertWasNotCalled(r => r.CheckForActivityChange(Guid.Empty, Guid.Empty, DateTime.UtcNow), a => a.IgnoreArguments());
		}

		[Test]
		public void PersonActivityStarting_PersonHaveExternalLogOn_ShouldCheckAgain()
		{
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			target = new PersonActivityChangePulseLoop(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService,
			                                           personRepository);
			var message = new PersonActivityChangePulseEvent {PersonHaveExternalLogOn = true};

			target.Handle(message);

			personRepository.AssertWasNotCalled(r => r.DoesPersonHaveExternalLogOn(DateOnly.Today, Guid.Empty), a => a.IgnoreArguments());
		}

	}
}
