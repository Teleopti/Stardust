using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class UpdatedScheduleInfoConsumerTest
	{
		//    private MockRepository mocks;
		//    private IServiceBus serviceBus;
		//    private IUnitOfWorkFactory unitOfWorkFactory;
		//    private IUnitOfWork unitOfWork;
		//    private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		//    private UpdatedScheduleInfoConsumer target;

		//    [SetUp]
		//    public void Setup()
		//    {
		//        mocks = new MockRepository();
		//        serviceBus = mocks.DynamicMock<IServiceBus>();
		//        unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
		//        unitOfWork = mocks.DynamicMock<IUnitOfWork>();
		//        scheduleProjectionReadOnlyRepository = new ScheduleProjectionReadOnlyRepository(unitOfWorkFactory);
		//        //mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();

		//        target = new UpdatedScheduleInfoConsumer(serviceBus, scheduleProjectionReadOnlyRepository, unitOfWorkFactory);
		//    }

		//    [Test]
		//    public void IsPersonWithExternalLogOnConsumeSuccessfully()
		//    {
		//        //var person = PersonFactory.CreatePerson();
		//        //person.SetId(Guid.NewGuid());

		//        //var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
		//        //bussinessUnit.SetId(Guid.NewGuid());

		//        //var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

		//        //var personInfoMessage = new PersonWithExternalLogOn();
		//        //personInfoMessage.PersonId = person.Id.GetValueOrDefault();
		//        //personInfoMessage.Timestamp = period.StartDateTime;
		//        //personInfoMessage.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
		//        //personInfoMessage.Datasource = "DS";

		//        //Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);

		//        //mocks.ReplayAll();
		//        //target.Consume(personInfoMessage);
		//        //mocks.VerifyAll();
		//    }

		//    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		//    public void DoNotSendDelayMessageIfThereIsNoNextActivity()
		//    {

		//    }

		//    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		//    public void SendDelayMessageIfThereExistNextActivity()
		//    {

		//    }

		//    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		//    public void IsUpdatedScheduleDayConsumeSuccessfully()
		//    {

		//    }

		//    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		//    public void DoNotSendAnyMessageIfTheScheduleIsNotUpdatedWithinRange()
		//    {

		//    }

		//    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		//    public void SendMessageIfTheScheduleIsNotUpdatedWithinRange()
		//    {

		//    }
		//}
	}
}
