using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
    [TestFixture]
    [DomainTest]
    [Toggle(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
    public class IntradayResourceCalculationForAbsenceHandlerTest : ISetup
    {
        public IntradayResourceCalculationForAbsenceHandler Target;
        public FakeBusinessUnitRepository BusinessUnitRepository;
        public FakeConfigReader ConfigReader;
        public FakePersonRepository PersonRepository;
        public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
        public FakeEventPublisher Publisher;
        public IMutateNow Now;
	    public FakeCurrentBusinessUnit BuesinessUnitScope;

		public void Setup(ISystem system, IIocConfiguration configuration)
        {
            system.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
            system.UseTestDouble<IntradayResourceCalculationForAbsenceHandler>().For<IHandleEvent<TenantMinuteTickEvent>>();
            system.UseTestDouble<FakeConfigReader>().For<IConfigReader>();
            system.UseTestDouble(new MutableNow("2016-03-01 10:00")).For<INow>();
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<IBusinessUnitScope>();
        }

        [Test]
        public void ShouldNotRunResourceCalculationIfItsRecentlyExecuted()
        {
            ScheduleForecastSkillReadModelRepository.LastCalculatedDate = DateTime.Now;
            Target.Handle(new TenantMinuteTickEvent());
            Publisher.PublishedEvents.Count().Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldRunResourceCalculation()
        {
            ScheduleForecastSkillReadModelRepository.LastCalculatedDate = new DateTime(2016, 03, 01, 8, 0, 0, DateTimeKind.Utc);
            BusinessUnitRepository.Add(BusinessUnitFactory.CreateSimpleBusinessUnit());
            IPerson person = PersonFactory.CreatePerson();
            person.SetId(SystemUser.Id);
            PersonRepository.Add(person);
            Target.Handle(new TenantMinuteTickEvent());
            Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldUseConfiguredFakeNowInsteadOfSystemNow()
        {
            ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-02-01 08:10");
            ScheduleForecastSkillReadModelRepository.LastCalculatedDate = new DateTime(2016, 03, 01, 8, 0, 0, DateTimeKind.Utc);
			BusinessUnitRepository.Add(BusinessUnitFactory.CreateSimpleBusinessUnit());
			IPerson person = PersonFactory.CreatePerson();
			person.SetId(SystemUser.Id);
			PersonRepository.Add(person);
			Target.Handle(new TenantMinuteTickEvent());
            Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
	        var StardustEvent = Publisher.PublishedEvents.FirstOrDefault() as UpdateResourceCalculateReadModelEvent;
	        StardustEvent.StartDateTime.Should().Be.EqualTo(new DateTime(2016, 02, 01, 8, 10, 0, DateTimeKind.Utc));

        }

		[Test]
		public void ShouldResetTheExistingValueOfBusinessUnit()
		{
			BuesinessUnitScope.OnThisThreadUse(null);
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-02-01 08:10");
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate = new DateTime(2016, 03, 01, 8, 0, 0, DateTimeKind.Utc);
			BusinessUnitRepository.Add(BusinessUnitFactory.CreateSimpleBusinessUnit());
			IPerson person = PersonFactory.CreatePerson();
			person.SetId(SystemUser.Id);
			PersonRepository.Add(person);
			Target.Handle(new TenantMinuteTickEvent());
			BuesinessUnitScope.Current().Should().Be.Null();
		}

	}
}
