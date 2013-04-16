using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;
using Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class UpdatedScheduleInfoConsumerTest
	{
		private MockRepository mocks;
		private ISendDelayedMessages serviceBus;
		private ICurrentUnitOfWorkFactory _currentunitOfWorkFactory;
		private IUnitOfWork unitOfWork;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private UpdatedScheduleInfoConsumer target;
		private IGetUpdatedScheduleChangeFromTeleoptiRtaService teleoptiRtaService;

		[SetUp]
		public void Setup()
		{
		    mocks = new MockRepository();
		    serviceBus = mocks.StrictMock<ISendDelayedMessages>();
			_currentunitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
		    unitOfWork = mocks.DynamicMock<IUnitOfWork>();
		    scheduleProjectionReadOnlyRepository = mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
			teleoptiRtaService = mocks.DynamicMock<IGetUpdatedScheduleChangeFromTeleoptiRtaService>();
            target = new UpdatedScheduleInfoConsumer(serviceBus, scheduleProjectionReadOnlyRepository, teleoptiRtaService);
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
	}
}
