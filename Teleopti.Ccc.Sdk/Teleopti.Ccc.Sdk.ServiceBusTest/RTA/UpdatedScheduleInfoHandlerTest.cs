using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class UpdatedScheduleInfoHandlerTest
	{
		private FakeRecordingDelayedMessageSender serviceBus;
		private FakeScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private IPersonRepository personRepository;
		private PersonActivityChangePulseLoop target;
		private FakeNotifyRtaToCheckForActivityChange teleoptiRtaService;

		[SetUp]
		public void Setup()
		{
			serviceBus = new FakeRecordingDelayedMessageSender();
			scheduleProjectionReadOnlyRepository = new FakeScheduleProjectionReadOnlyRepository();
			teleoptiRtaService = new FakeNotifyRtaToCheckForActivityChange();
			personRepository = new FakePersonRepository();
			target = new PersonActivityChangePulseLoop(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService, personRepository);
		}

		[Test]
		public void IsPersonActivityStartingConsumeSuccessfully()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

            var personInfoMessage = new PersonActivityChangePulseEvent
				{
					PersonId = person.Id.GetValueOrDefault(),
					Timestamp = period.StartDateTime,
					LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
					LogOnDatasource = "DS"
				};
			scheduleProjectionReadOnlyRepository.SetNextActivityStartTime(person, DateTime.UtcNow.AddHours(3));
			
			target.Handle(personInfoMessage);
			serviceBus.SendCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void DoNotSendDelayMessageIfThereIsNoNextActivity()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			
			target.Handle(updatedSchduleDay);
			serviceBus.SendCount.Should().Be.EqualTo(0);
		}
		
		[Test]
		public void SendDelayMessageIfThereExistNextActivity()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
           {
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};

			scheduleProjectionReadOnlyRepository.SetNextActivityStartTime(person, DateTime.UtcNow.AddHours(3));

			target.Handle(updatedSchduleDay);
			serviceBus.SendCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void DoNotSendAnyMessageIfTheScheduleIsNotUpdatedWithinRange()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddDays(3),
				ActivityEndDateTime = DateTime.UtcNow.AddDays(4),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};

			scheduleProjectionReadOnlyRepository.SetNextActivityStartTime(person, DateTime.UtcNow.AddDays(2));

			target.Handle(updatedSchduleDay);
			serviceBus.SendCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void SendMessageIfTheScheduleIsUpdatedWithinRange()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			scheduleProjectionReadOnlyRepository.SetNextActivityStartTime(person, DateTime.UtcNow.AddHours(3));
			
			target.Handle(updatedSchduleDay);

			serviceBus.SendCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_NoNextActivity_ShouldNotEnqueue()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
				{
					ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
					ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
					PersonId = person.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
				};
			
			target.Handle(updatedSchduleDay);
			serviceBus.SendCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_NextActivityBeforeScheduleChange_ShouldNotEnqueue()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(4),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			scheduleProjectionReadOnlyRepository.SetNextActivityStartTime(person, DateTime.UtcNow.AddHours(1));
			
			target.Handle(updatedSchduleDay);
			serviceBus.SendCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_NextActivityAfterScheduleChange_ShouldEnqueue()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			scheduleProjectionReadOnlyRepository.SetNextActivityStartTime(person, DateTime.UtcNow.AddHours(4));
			
			target.Handle(updatedSchduleDay);
			serviceBus.SendCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_PersonDoesNotHaveExternalLogOn_ShouldDiscard()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			personRepository.Add(person);

			var updatedScheuldeDay = new ScheduleProjectionReadOnlyChanged
				{
					ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
					ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
					PersonId = person.Id.GetValueOrDefault(),
					LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
				};
			
			target.Handle(updatedScheuldeDay);
			
			teleoptiRtaService.CheckCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ScheduleProjectionReadOnlyChanged_PersonHaveExternalLogOn_ShouldSend()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			person.AddExternalLogOn(new ExternalLogOn(), person.Period(DateOnly.Today.AddDays(-1)));
			personRepository.Add(person);

			var updatedScheuldeDay = new ScheduleProjectionReadOnlyChanged
			{
				ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
				ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
				PersonId = person.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			
			target.Handle(updatedScheuldeDay);

			teleoptiRtaService.CheckCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void PersonActivityStarting_PersonDoesNotHaveExternalLogOn_ShouldDiscard()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today.AddDays(-1)).WithId();
			personRepository.Add(person);

			var personActivityStarting = new PersonActivityChangePulseEvent
				{
					PersonId = Guid.NewGuid(),
					LogOnBusinessUnitId = Guid.Empty,
					Timestamp = DateTime.UtcNow
				};
			
			target.Handle(personActivityStarting);

			teleoptiRtaService.CheckCount.Should().Be.EqualTo(0);
		}
	}

	public class FakeRecordingDelayedMessageSender : IDelayedMessageSender
	{
		public int SendCount;

		public void DelaySend(DateTime time, object message)
		{
			System.Threading.Interlocked.Increment(ref SendCount);
		}
	}

	public class FakeNotifyRtaToCheckForActivityChange : INotifyRtaToCheckForActivityChange
	{
		public int CheckCount;

		public Task CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			System.Threading.Interlocked.Increment(ref CheckCount);
			return Task.FromResult(false);
		}
	}
}
