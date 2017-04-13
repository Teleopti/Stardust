using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsPersonNameUpdaterTests : ISetup
	{
		public AnalyticsPersonNameUpdater Target;
		public IAnalyticsPersonPeriodRepository Repository;
		public IGlobalSettingDataRepository GlobalSettingDataRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AnalyticsPersonNameUpdater>().For<AnalyticsPersonNameUpdater>();
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
		}

		[Test]
		public void ShouldRunUpdateOfNames()
		{
			GlobalSettingDataRepository.PersistSettingValue(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting());
			var personPeriodCode = Guid.NewGuid();
			Repository.AddPersonPeriod(new AnalyticsPersonPeriod { PersonPeriodCode = personPeriodCode, FirstName = "Erik" });
			Repository.PersonPeriod(personPeriodCode).PersonName.Should().Be.Null();
			Target.Handle(new CommonNameDescriptionChangedEvent());
			Repository.PersonPeriod(personPeriodCode).PersonName.Should().Not.Be.Null();
		}
	}
}