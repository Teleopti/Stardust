using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.ApplicationRtaQueue;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class UpdatedScheduleInfoHandlerTest
	{
		private MockRepository mocks;
		private ISendDelayedMessages serviceBus;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private UpdatedScheduleInfoHandler target;
		private IGetUpdatedScheduleChangeFromTeleoptiRtaService teleoptiRtaService;
		private DateTime utcNow;
		private Guid personId, businessUntiId;

		[SetUp]
		public void Setup()
		{
		    mocks = new MockRepository();
		    serviceBus = mocks.StrictMock<ISendDelayedMessages>();
		    scheduleProjectionReadOnlyRepository = mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
			teleoptiRtaService = mocks.DynamicMock<IGetUpdatedScheduleChangeFromTeleoptiRtaService>();

			utcNow = DateTime.UtcNow;
			personId = Guid.NewGuid();
			businessUntiId = Guid.NewGuid();

            target = new UpdatedScheduleInfoHandler(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService);
        }

        [Test]
        public void IsPersonWithExternalLogOnConsumeSuccessfully()
        {
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
            bussinessUnit.SetId(Guid.NewGuid());

            var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

            var personInfoMessage = new PersonWithExternalLogOn
                {
                    PersonId = person.Id.GetValueOrDefault(),
                    Timestamp = period.StartDateTime,
                    BusinessUnitId = bussinessUnit.Id.GetValueOrDefault(),
                    Datasource = "DS"
                };

            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                      person.Id.GetValueOrDefault())).IgnoreArguments()
                  .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(()=>serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

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

            var updatedSchduleDay = new UpdatedScheduleDay
            {
                ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
                ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
                PersonId = person.Id.GetValueOrDefault(),
                BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
            };

            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                     person.Id.GetValueOrDefault())).IgnoreArguments()
                 .Return(null);

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

            var updatedSchduleDay = new UpdatedScheduleDay
            {
                ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
                ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
                PersonId = person.Id.GetValueOrDefault(),
                BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
            };
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                    person.Id.GetValueOrDefault())).IgnoreArguments()
                .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

            mocks.ReplayAll();
            target.Handle(updatedSchduleDay);
            mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void IsUpdatedScheduleDayConsumeSuccessfully()
        {
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
            bussinessUnit.SetId(Guid.NewGuid());

            var updatedSchduleDay = new UpdatedScheduleDay
                {
                    ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
                    ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
                    PersonId = person.Id.GetValueOrDefault(),
                    BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
                };
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                     person.Id.GetValueOrDefault())).IgnoreArguments()
                 .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

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

            var updatedSchduleDay = new UpdatedScheduleDay
            {
                ActivityStartDateTime = DateTime.UtcNow.AddDays(3),
                ActivityEndDateTime = DateTime.UtcNow.AddDays(4),
                PersonId = person.Id.GetValueOrDefault(),
                BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
            };
	        Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
	                                                                                  person.Id.GetValueOrDefault()))
	              .IgnoreArguments().Return(DateTime.UtcNow.AddDays(2));

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

            var updatedSchduleDay = new UpdatedScheduleDay
            {
                ActivityStartDateTime = DateTime.UtcNow.AddHours(1),
                ActivityEndDateTime = DateTime.UtcNow.AddHours(8),
                PersonId = person.Id.GetValueOrDefault(),
                BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
            };
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                     person.Id.GetValueOrDefault())).IgnoreArguments()
                 .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

            mocks.ReplayAll();
            target.Handle(updatedSchduleDay);
            mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDayHandler_ChangeIsBeforeNextActivityStartTime_ShouldSend()
		{
			var message = new UpdatedScheduleDay
				{
					PersonId = personId,
					BusinessUnitId = businessUntiId,
					ActivityStartDateTime = utcNow.AddDays(3),
					ActivityEndDateTime = utcNow.AddDays(4),
					Datasource = "2",
					Timestamp = utcNow
				};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(utcNow, personId))
			      .IgnoreArguments()
			      .Return(utcNow.AddDays(4));
			Expect.Call(() =>serviceBus.DelaySend(utcNow.AddDays(4), null)).IgnoreArguments();
			mocks.ReplayAll();
			target.Handle(message);
			mocks.VerifyAll();
		}
		 
		[Test]
		public void UpdatedScheduleDayHandler_ChangeIsAfterNextActivityStartTime_ShouldNotSend()
		{
			var message = new UpdatedScheduleDay
			{
				PersonId = personId,
				BusinessUnitId = businessUntiId,
				ActivityStartDateTime = utcNow.AddDays(3),
				ActivityEndDateTime = utcNow.AddDays(4),
				Datasource = "2",
				Timestamp = utcNow
			};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(utcNow, personId)).IgnoreArguments().Return(utcNow);
			mocks.ReplayAll();
			target.Handle(message);
			mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDayHandler_NoNextActivity_ShouldNotCrash()
		{
			var message = new UpdatedScheduleDay
			{
				PersonId = personId,
				BusinessUnitId = businessUntiId,
				ActivityStartDateTime = utcNow.AddDays(3),
				ActivityEndDateTime = utcNow.AddDays(4),
				Datasource = "2",
				Timestamp = utcNow
			};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(utcNow, personId)).IgnoreArguments().Return(utcNow);
			mocks.ReplayAll();
			target.Handle(message);
			mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDayHandler_NextActivityWithinTwoDays_SendToRta()
		{
			var message = new UpdatedScheduleDay
				{
					PersonId = personId,
					BusinessUnitId = businessUntiId,
					ActivityStartDateTime = utcNow.AddDays(1),
					ActivityEndDateTime = utcNow.AddDays(2),
					Datasource = "2",
					Timestamp = utcNow
				};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(utcNow, personId))
			      .IgnoreArguments()
			      .Return(utcNow);
			mocks.ReplayAll();
			target.Handle(message);
			mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDayHandler_MessageWithinEndRange_SendToRta()
		{
			var message = new UpdatedScheduleDay
				{
					BusinessUnitId = businessUntiId,
					PersonId = personId,
					ActivityStartDateTime = utcNow.AddDays(13),
					ActivityEndDateTime = utcNow.AddDays(14),
					Datasource = "2",
					Timestamp = utcNow
				};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(utcNow, personId))
			      .IgnoreArguments()
			      .Return(utcNow.AddDays(11));
			Expect.Call(() => teleoptiRtaService.GetUpdatedScheduleChange(personId, businessUntiId, utcNow))
			      .IgnoreArguments();

			mocks.ReplayAll();
			target.Handle(message);
			mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDayHandler_MessageWithinStartRange_SendToRta()
		{
			var message = new UpdatedScheduleDay
			{
				BusinessUnitId = businessUntiId,
				PersonId = personId,
				ActivityStartDateTime = utcNow.AddDays(8),
				ActivityEndDateTime = utcNow.AddDays(9),
				Datasource = "2",
				Timestamp = utcNow
			};

			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(utcNow, personId))
				  .IgnoreArguments()
				  .Return(utcNow.AddDays(11));
			Expect.Call(() => teleoptiRtaService.GetUpdatedScheduleChange(personId, businessUntiId, utcNow))
				  .IgnoreArguments();
			Expect.Call(() => serviceBus.DelaySend(utcNow, null))
			      .IgnoreArguments();

			mocks.ReplayAll();
			target.Handle(message);
			mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDayHandler_MessageBeforeNow_DoNotSend()
		{

			var message = new UpdatedScheduleDay
			{
				BusinessUnitId = businessUntiId,
				PersonId = personId,
				ActivityStartDateTime = utcNow.AddDays(-2),
				ActivityEndDateTime = utcNow.AddDays(-1),
				Datasource = "2",
				Timestamp = utcNow
			};
			
			mocks.ReplayAll();
			target.Handle(message);
			mocks.VerifyAll();
		}

		[Test]
		public void Coverage()
		{
			target = new UpdatedScheduleInfoHandler(MockRepository.GenerateMock<ISendDelayedMessages>(), MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>(), MockRepository.GenerateMock<IGetUpdatedScheduleChangeFromTeleoptiRtaService>());
			target.Handle(new PersonWithExternalLogOn());

			var repo = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			repo.Stub(x => x.GetNextActivityStartTime(DateTime.MinValue, Guid.Empty)).IgnoreArguments().Return(DateTime.Now);
			var service = MockRepository.GenerateMock<IGetUpdatedScheduleChangeFromTeleoptiRtaService>();
			service.Stub(x => x.GetUpdatedScheduleChange(Guid.Empty, Guid.Empty, DateTime.MinValue)).IgnoreArguments().Throw(new Exception());
			target = new UpdatedScheduleInfoHandler(MockRepository.GenerateMock<ISendDelayedMessages>(), repo, service);
			target.Handle(new PersonWithExternalLogOn());

			target = new UpdatedScheduleInfoHandler(MockRepository.GenerateMock<ISendDelayedMessages>(), repo, service);
			target.Handle(new UpdatedScheduleDay
				{
					ActivityStartDateTime = DateTime.UtcNow.AddHours(-1),
					ActivityEndDateTime = DateTime.UtcNow.AddHours(1)
				});

			new IgnoreGetUpdatedScheduleChangeFromTeleoptiRtaService().GetUpdatedScheduleChange(Guid.Empty, Guid.Empty, DateTime.MinValue);
			Assert.Throws<NotImplementedException>(() => new CannotGetUpdatedScheduleChangeFromTeleoptiRtaService().GetUpdatedScheduleChange(Guid.Empty, Guid.Empty, DateTime.MinValue));

			new IgnoreDelayedMessages().DelaySend(DateTime.MinValue, new object());
			Assert.Throws<NotImplementedException>(() => new CannotSendDelayedMessages().DelaySend(DateTime.MinValue, new object()));
		}

	}
}
