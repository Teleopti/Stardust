using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
    [TestFixture]
    [DomainTest]
    public class SendUpdateStaffingReadModelHandlerTest : ISetup
    {
        public SendUpdateStaffingReadModelHandler Target;
        public FakeBusinessUnitRepository BusinessUnitRepository;
        public FakeConfigReader ConfigReader;
        public FakePersonRepository PersonRepository;
        public FakeEventPublisher Publisher;
        public MutableNow Now;
	    private FakeCurrentBusinessUnit _fakeCurrentBusniessUnit;
	    public FakeJobStartTimeRepository JobStartTimeRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
        {
			_fakeCurrentBusniessUnit = new FakeCurrentBusinessUnit();
			system.UseTestDouble(_fakeCurrentBusniessUnit).For<IBusinessUnitScope>();
			system.UseTestDouble(_fakeCurrentBusniessUnit).For<ICurrentBusinessUnit>();
        }

		[Test]
		public void ShouldNotRunResourceCalculationIfItsRecentlyExecuted()
		{
			Now.Is("2016-03-01 09:50");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			addPerson();
			_fakeCurrentBusniessUnit.OnThisThreadUse(bu);
			JobStartTimeRepository.CheckAndUpdate(60);
			_fakeCurrentBusniessUnit.OnThisThreadUse(null);
			Now.Is("2016-03-01 10:00");
			Target.Handle(new TenantMinuteTickEvent());
			Publisher.PublishedEvents.Count().Should().Be.EqualTo(0);
		}

		[Test]
        public void ShouldRunResourceCalculation()
        {
			Now.Is("2016-03-01 10:00");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			//_fakeCurrentBusniessUnit.OnThisThreadUse(bu);
			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
            Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
        }

		[Test]
		public void ShouldResetTheExistingValueOfBusinessUnit()
		{
			Now.Is("2016-03-01 10:00");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			//_fakeCurrentBusniessUnit.OnThisThreadUse(bu);
			BusinessUnitRepository.Add(bu);
			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
			_fakeCurrentBusniessUnit.Current().Should().Be.Null();
		}

		[Test]
		public void ShouldExecuteJobOnlyFor1Bu()
		{
			Now.Is("2016-03-01 06:10");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			var bu2 = BusinessUnitFactory.CreateWithId("B2");
			BusinessUnitRepository.Add(bu);
			BusinessUnitRepository.Add(bu2);

			_fakeCurrentBusniessUnit.OnThisThreadUse(bu);
			JobStartTimeRepository.CheckAndUpdate(60);
			Now.Is("2016-03-01 08:10");
			JobStartTimeRepository.CheckAndUpdate(60);
			_fakeCurrentBusniessUnit.OnThisThreadUse(null);
			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
			Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishEventForBuIfNotInReadModel()
		{
			Now.Is("2016-03-01 08:10");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			//_fakeCurrentBusniessUnit.OnThisThreadUse(bu);
			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
			Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

	    private void addPerson()
	    {
		    var person = PersonFactory.CreatePerson();
		    person.SetId(SystemUser.Id);
		    PersonRepository.Add(person);
	    }
    }
}
