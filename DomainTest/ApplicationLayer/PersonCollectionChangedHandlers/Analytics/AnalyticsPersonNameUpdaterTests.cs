using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsPersonNameUpdaterTests : IIsolateSystem
	{
		public AnalyticsPersonNameUpdater Target;
		public IAnalyticsPersonPeriodRepository Repository;
		public IGlobalSettingDataRepository GlobalSettingDataRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AnalyticsPersonNameUpdater>().For<AnalyticsPersonNameUpdater>();
		}

		[Test]
		public void ShouldRunUpdateOfNames()
		{
			GlobalSettingDataRepository.PersistSettingValue(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting());
			var personPeriodCode = Guid.NewGuid();
			Repository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod { PersonPeriodCode = personPeriodCode, FirstName = "Erik" });
			Repository.PersonPeriod(personPeriodCode).PersonName.Should().Be.Null();
			Target.Handle(new CommonNameDescriptionChangedEvent());
			Repository.PersonPeriod(personPeriodCode).PersonName.Should().Not.Be.Null();
		}
	}
}