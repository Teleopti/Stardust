using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
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

		[SetUp]
		public void Setup()
		{
		    mocks = new MockRepository();
		    serviceBus = mocks.StrictMock<ISendDelayedMessages>();
		    scheduleProjectionReadOnlyRepository = mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
			teleoptiRtaService = mocks.DynamicMock<IGetUpdatedScheduleChangeFromTeleoptiRtaService>();

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

            var personInfoMessage = new PersonActivityStarting
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

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
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

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
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

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
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

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
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

            var updatedSchduleDay = new ScheduleProjectionReadOnlyChanged
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
		public void UpdatedScheduleDay_NoNextActivity_ShouldNotEnqueue()
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
					BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
				};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
			                                                                          person.Id.GetValueOrDefault()))
			      .IgnoreArguments()
			      .Return(null);

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDay_NextActivityBeforeScheduleChange_ShouldNotEnqueue()
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
				BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					  person.Id.GetValueOrDefault()))
				  .IgnoreArguments()
				  .Return(DateTime.UtcNow.AddHours(1));

			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[Test]
		public void UpdatedScheduleDay_NextActivityAfterScheduleChange_ShouldEnqueue()
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
				BusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																					  person.Id.GetValueOrDefault()))
				  .IgnoreArguments()
				  .Return(DateTime.UtcNow.AddHours(4));
			Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow, null))
			      .IgnoreArguments();


			mocks.ReplayAll();
			target.Handle(updatedSchduleDay);
			mocks.VerifyAll();
		}

		[Test]
		public void Coverage()
		{
			target = new UpdatedScheduleInfoHandler(MockRepository.GenerateMock<ISendDelayedMessages>(), MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>(), MockRepository.GenerateMock<IGetUpdatedScheduleChangeFromTeleoptiRtaService>());
			target.Handle(new PersonActivityStarting());

			var repo = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			repo.Stub(x => x.GetNextActivityStartTime(DateTime.MinValue, Guid.Empty)).IgnoreArguments().Return(DateTime.Now);
			var service = MockRepository.GenerateMock<IGetUpdatedScheduleChangeFromTeleoptiRtaService>();
			service.Stub(x => x.GetUpdatedScheduleChange(Guid.Empty, Guid.Empty, DateTime.MinValue)).IgnoreArguments().Throw(new Exception());
			target = new UpdatedScheduleInfoHandler(MockRepository.GenerateMock<ISendDelayedMessages>(), repo, service);
			target.Handle(new PersonActivityStarting());

			target = new UpdatedScheduleInfoHandler(MockRepository.GenerateMock<ISendDelayedMessages>(), repo, service);
			target.Handle(new ScheduleProjectionReadOnlyChanged
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
