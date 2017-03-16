using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
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
    public class IntradayResourceCalculationForAbsenceHandlerTest : ISetup
    {
        public SendUpdateStaffingReadModelHandler Target;
        public FakeBusinessUnitRepository BusinessUnitRepository;
        public FakeConfigReader ConfigReader;
        public FakePersonRepository PersonRepository;
        public FakeEventPublisher Publisher;
        public MutableNow Now;
	    public FakeCurrentBusinessUnit BuesinessUnitScope;
	    public FakeJobStartTimeRepository JobStartTimeRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
        {
            system.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
            system.UseTestDouble<SendUpdateStaffingReadModelHandler>().For<IHandleEvent<TenantMinuteTickEvent>>();
            system.UseTestDouble<FakeConfigReader>().For<IConfigReader>();
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<IBusinessUnitScope>();
			system.UseTestDouble<FakeJobStartTimeRepository>().For<IJobStartTimeRepository>();
        }

        [Test]
        public void ShouldNotRunResourceCalculationIfItsRecentlyExecuted()
        {
			Now.Is("2016-03-01 10:00");
            Target.Handle(new TenantMinuteTickEvent());
            Publisher.PublishedEvents.Count().Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldRunResourceCalculation()
        {
			Now.Is("2016-03-01 10:00");
	        var businessUnit = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(businessUnit);
			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
            Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
        }

		[Test]
		public void ShouldResetTheExistingValueOfBusinessUnit()
		{
			Now.Is("2016-03-01 10:00");
			BuesinessUnitScope.OnThisThreadUse(null);
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-02-01 08:10");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);

			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
			BuesinessUnitScope.Current().Should().Be.Null();
		}

		[Test]
		public void ShouldExecuteJobOnlyFor1Bu()
		{
			Now.Is("2016-03-01 08:10");
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-01 08:10");
			var bu1 = BusinessUnitFactory.CreateWithId("B1");
			var bu2 = BusinessUnitFactory.CreateWithId("B2");
			BusinessUnitRepository.Add(bu1);
			BusinessUnitRepository.Add(bu2);
			JobStartTimeRepository.Persist(bu1.Id.GetValueOrDefault(), new DateTime(2016, 03, 01, 6, 30, 0, DateTimeKind.Utc));
			JobStartTimeRepository.Persist(bu2.Id.GetValueOrDefault(), new DateTime(2016, 03, 01, 8, 9, 0, DateTimeKind.Utc));

			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
			Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishEventForBuIfNotInReadModel()
		{
			Now.Is("2016-03-01 08:10");
			var bu1 = BusinessUnitFactory.CreateWithId("B1");
			BusinessUnitRepository.Add(bu1);
			addPerson();
			Target.Handle(new TenantMinuteTickEvent());
			Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

	    private void addPerson()
	    {
		    IPerson person = PersonFactory.CreatePerson();
		    person.SetId(SystemUser.Id);
		    PersonRepository.Add(person);
	    }
    }
}
