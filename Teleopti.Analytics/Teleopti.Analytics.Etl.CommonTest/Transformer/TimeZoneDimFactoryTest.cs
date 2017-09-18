using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Transformer;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class TimeZoneDimFactoryTest
	{
		private readonly TimeZoneDimFactory _target = new TimeZoneDimFactory();

		[Test]
		public void ShouldCreateDefaultTimeZone()
		{
			var timeZoneDimList = _target.Create(TimeZoneInfo.Utc, new List<TimeZoneInfo>(), new List<TimeZoneInfo>());

			timeZoneDimList.Count.Should().Be.EqualTo(1);
			timeZoneDimList.Any(t => t.TimeZoneCode == TimeZoneInfo.Utc.Id).Should().Be.True();
		}

		[Test]
		public void ShouldCreateFromTimeZonesUsedByClient()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZoneDimList = _target.Create(TimeZoneInfo.Utc, new List<TimeZoneInfo> { timeZone }, new List<TimeZoneInfo>());

			timeZoneDimList.Count.Should().Be.EqualTo(2);
			timeZoneDimList.Any(t => t.TimeZoneCode == TimeZoneInfo.Utc.Id).Should().Be.True();
			timeZoneDimList.Any(t => t.TimeZoneCode == timeZone.Id).Should().Be.True();
		}

		[Test]
		public void ShouldCreateFromTimeZonesUsedByDataSource()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZoneDimList = _target.Create(TimeZoneInfo.Utc, new List<TimeZoneInfo>(), new List<TimeZoneInfo> { timeZone });

			timeZoneDimList.Count.Should().Be.EqualTo(2);
			timeZoneDimList.Any(t => t.TimeZoneCode == TimeZoneInfo.Utc.Id).Should().Be.True();
			timeZoneDimList.Any(t => t.TimeZoneCode == timeZone.Id).Should().Be.True();
		}

		[Test]
		public void ShouldSetTimeZonesAsDefault()
		{
			var timeZoneDimList = _target.Create(TimeZoneInfo.Utc, new List<TimeZoneInfo>(), new List<TimeZoneInfo>());

			timeZoneDimList.Count.Should().Be.EqualTo(1);
			timeZoneDimList.Any(t => t.TimeZoneCode == TimeZoneInfo.Utc.Id).Should().Be.True();
			timeZoneDimList.First().IsDefaultTimeZone.Should().Be.True();
		}

		[Test]
		public void ShouldCreateFromTimeZonesUseByClientAndDataSourcesWithoutDuplicates()
		{
			var westEuropeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZonesUsedByClient = new List<TimeZoneInfo> { TimeZoneInfo.Utc };
			var timeZonesUsedByDataSources = new List<TimeZoneInfo> { TimeZoneInfo.Utc, westEuropeTimeZone };
			var timeZoneDimList = _target.Create(TimeZoneInfo.Utc, timeZonesUsedByClient, timeZonesUsedByDataSources);

			timeZoneDimList.Count.Should().Be.EqualTo(2);
			timeZoneDimList.Any(t => t.TimeZoneCode == TimeZoneInfo.Utc.Id).Should().Be.True();
			timeZoneDimList.Any(t => t.TimeZoneCode == westEuropeTimeZone.Id).Should().Be.True();
		}

		[Test]
		public void ShouldEnsureUtcAlwaysIsCreated()
		{
			var westEuropeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZonesUsedByClient = new List<TimeZoneInfo> { westEuropeTimeZone };
			var timeZonesUsedByDataSources = new List<TimeZoneInfo> { westEuropeTimeZone };
			var timeZoneDimList = _target.Create(westEuropeTimeZone, timeZonesUsedByClient, timeZonesUsedByDataSources);

			timeZoneDimList.Count.Should().Be.EqualTo(2);
			timeZoneDimList.Any(t => t.TimeZoneCode == westEuropeTimeZone.Id).Should().Be.True();
			var utcTimeZoneDim = timeZoneDimList.Single(t => t.TimeZoneCode == TimeZoneInfo.Utc.Id);
			utcTimeZoneDim.IsUtcInUse.Should().Be.False();
		}

		[Test]
		public void ShouldSetAsUtcInUseByClient()
		{
			var usedByClient = TimeZoneInfo.Utc;
			var etlDefaultTimeZone = usedByClient;
			var timeZoneDimList = _target.Create(etlDefaultTimeZone, new List<TimeZoneInfo> { usedByClient }, new List<TimeZoneInfo>());

			timeZoneDimList.Count.Should().Be.EqualTo(1);
			timeZoneDimList.First().TimeZoneCode.Should().Be.EqualTo(usedByClient.Id);
			timeZoneDimList.First().IsUtcInUse.Should().Be.EqualTo(true);
		}
	}
}
