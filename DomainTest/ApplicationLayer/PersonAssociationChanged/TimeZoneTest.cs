using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[DomainTest]
	[AddDatasourceId]
	public class TimeZoneTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;
		
		[Test]
		public void ShouldPublishWhenTerminadedInIstanbul()
		{
			Data.WithAgent("pierre", "2016-01-14", TimeZoneInfoFactory.IstanbulTimeZoneInfo());
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-01-14 22:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenTerminationDateOccursOnDst()
		{
			Data.WithAgent("Pierre", "2016-03-20", TimeZoneInfoFactory.IranTimeZoneInfo());
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-03-21 00:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenTerminationDateOccursOnDst2()
		{
			Data.WithAgent("Pierre", "2016-03-21", TimeZoneInfoFactory.IranTimeZoneInfo());
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-03-22 00:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}
	}
}