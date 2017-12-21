using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsTimeZoneUpdaterTest
	{
		public FakeAnalyticsTimeZoneRepository AnalyticsTimeZoneRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public AnalyticsTimeZoneUpdater Target;
		private Guid _businessUnitId;

		[SetUp]
		public void Setup()
		{
			_businessUnitId = Guid.NewGuid();
		}

		[Test]
		public void ShouldUpdateUtcInUse()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

			Target.Handle(new PossibleTimeZoneChangeEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			});

			var utcTimeZone = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == "UTC");
			var estAnalyticsTimeZone = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == "W. Europe Standard Time");
			utcTimeZone.IsUtcInUse.Should().Be.True();
			estAnalyticsTimeZone.IsUtcInUse.Should().Be.False();
		}

		[Test]
		public void ShouldMarkTimeZonesAsDeleted()
		{
			const string timeZoneInUse = "W. Europe Standard Time";
			const string timeZoneTobeDeleted = "GTB Standard Time";

			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInUse));

			AnalyticsTimeZoneRepository.Get(timeZoneTobeDeleted);

			Target.Handle(new PossibleTimeZoneChangeEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			});

			var inUse = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == timeZoneInUse);
			var deleted = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == timeZoneTobeDeleted);
			var utc = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == "UTC");
			inUse.ToBeDeleted.Should().Be.False();
			deleted.ToBeDeleted.Should().Be.True();
			utc.ToBeDeleted.Should().Be.False();
		}

		[Test]
		public void ShouldNotMarkTimeZonesAsDeletedWhenUsedOnlyByLogDataSourceOrBaseConfig()
		{
			const string timeZoneInUseByClient = "W. Europe Standard Time";
			const string timeZoneTobeDeleted = "GTB Standard Time";
			const string timeZoneForDataSource = "Russian Standard Time";

			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInUseByClient));

			AnalyticsTimeZoneRepository.Get(timeZoneTobeDeleted);
			AnalyticsTimeZoneRepository.Get(timeZoneForDataSource);
			AnalyticsTimeZoneRepository.HasLogDataSourceOrBaseConfigTimeZone(timeZoneForDataSource);

			Target.Handle(new PossibleTimeZoneChangeEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			});

			var inUseByClient = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == timeZoneInUseByClient);
			var inUseByDataSource = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == timeZoneForDataSource);
			var deleted = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == timeZoneTobeDeleted);
			var utc = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == "UTC");

			inUseByClient.ToBeDeleted.Should().Be.False();
			inUseByDataSource.ToBeDeleted.Should().Be.False();
			deleted.ToBeDeleted.Should().Be.True();
			utc.ToBeDeleted.Should().Be.False();
		}

		[Test]
		public void ShouldMarkTimeZoneAsNotDeleted()
		{
			const string timeZoneInUse = "W. Europe Standard Time";
			const string timeZoneDeleted = "GTB Standard Time";

			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			
			AnalyticsTimeZoneRepository.Get(timeZoneDeleted);
			AnalyticsTimeZoneRepository.SetToBeDeleted(timeZoneDeleted, true);

			var deleteTedTimeZone = AnalyticsTimeZoneRepository.Get(timeZoneDeleted);
			deleteTedTimeZone.ToBeDeleted.Should().Be.True();

			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInUse));
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneDeleted));

			Target.Handle(new PossibleTimeZoneChangeEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			});

			var readdedTimeZone = AnalyticsTimeZoneRepository.Get(timeZoneDeleted);
			readdedTimeZone.ToBeDeleted.Should().Be.False();
		}
	}
}