using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;
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
		private IServiceBus serviceBus;
		private ICurrentUnitOfWorkFactory _currentunitOfWorkFactory;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private UpdatedScheduleInfoConsumer target;
	    private ITeleoptiRtaService teleoptiRtaService;

		[SetUp]
		public void Setup()
		{
		    mocks = new MockRepository();
		    serviceBus = mocks.StrictMock<IServiceBus>();
			_currentunitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
		    unitOfWork = mocks.DynamicMock<IUnitOfWork>();
		    scheduleProjectionReadOnlyRepository = mocks.StrictMock<IScheduleProjectionReadOnlyRepository>();
            teleoptiRtaService = mocks.DynamicMock<ITeleoptiRtaService>();
            target = new UpdatedScheduleInfoConsumer(serviceBus, scheduleProjectionReadOnlyRepository, _currentunitOfWorkFactory, teleoptiRtaService);
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

            Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                      person.Id.GetValueOrDefault())).IgnoreArguments()
                  .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(()=>serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

            mocks.ReplayAll();
            target.Consume(personInfoMessage);
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

			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                     person.Id.GetValueOrDefault())).IgnoreArguments()
                 .Return(null);

            mocks.ReplayAll();
            target.Consume(updatedSchduleDay);
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
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                    person.Id.GetValueOrDefault())).IgnoreArguments()
                .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

            mocks.ReplayAll();
            target.Consume(updatedSchduleDay);
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
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                     person.Id.GetValueOrDefault())).IgnoreArguments()
                 .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

            mocks.ReplayAll();
            target.Consume(updatedSchduleDay);
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
            target.Consume(updatedSchduleDay);
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
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
                                                                                     person.Id.GetValueOrDefault())).IgnoreArguments()
                 .Return(DateTime.UtcNow.AddHours(3));
            Expect.Call(() => serviceBus.DelaySend(DateTime.UtcNow.AddDays(1), null)).IgnoreArguments();

            mocks.ReplayAll();
            target.Consume(updatedSchduleDay);
            mocks.VerifyAll();
		}
	}
}
