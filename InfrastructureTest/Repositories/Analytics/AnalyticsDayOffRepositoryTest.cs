using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsDayOffRepositoryTest
	{
		public IAnalyticsDayOffRepository Target;

		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);

			analyticsDataFactory.Setup(_datasource);
			analyticsDataFactory.Setup(new DimDayOff(-1, Guid.Empty, "Not Defined", _datasource, businessUnitId));
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldAddDayOff()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var dayOffCode = Guid.NewGuid();
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DayOffs().Count.Should().Be.EqualTo(1);
				var item = new AnalyticsDayOff
				{
					DayOffCode = dayOffCode,
					BusinessUnitId = businessUnitId,
					DayOffName = "Day off",
					DayOffShortname = "DO",
					DatasourceUpdateDate = DateTime.Today,
					DisplayColor = -1,
					DisplayColorHtml = "#808080"
				};
				Target.AddOrUpdate(item);
				Target.DayOffs().Count.Should().Be.EqualTo(2);
				var itemToCompare = Target.DayOffs().First(a => a.DayOffCode == dayOffCode);
				itemToCompare.BusinessUnitId.Should().Be.EqualTo(item.BusinessUnitId);
				itemToCompare.DayOffName.Should().Be.EqualTo(item.DayOffName);
				itemToCompare.DayOffShortname.Should().Be.EqualTo(item.DayOffShortname);
				itemToCompare.DatasourceUpdateDate.Should().Be.EqualTo(item.DatasourceUpdateDate);
				itemToCompare.DisplayColor.Should().Be.EqualTo(item.DisplayColor);
				itemToCompare.DisplayColorHtml.Should().Be.EqualTo(item.DisplayColorHtml);
			});
		}

		[Test]
		public void ShouldUpdateDayOff()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var dayOffCode = Guid.NewGuid();
			analyticsDataFactory.Setup(new DimDayOff(1, dayOffCode, "DayOff", _datasource, businessUnitId));
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DayOffs().Count.Should().Be.EqualTo(2);
				var item = new AnalyticsDayOff
				{
					DayOffCode = dayOffCode,
					BusinessUnitId = businessUnitId,
					DayOffName = "Day off updated",
					DayOffShortname = "DO",
					DatasourceUpdateDate = DateTime.Today,
					DisplayColor = -1,
					DisplayColorHtml = "#808080"
				};
				Target.AddOrUpdate(item);
				Target.DayOffs().Count.Should().Be.EqualTo(2);
				var itemToCompare = Target.DayOffs().First(a => a.DayOffCode == dayOffCode);
				itemToCompare.BusinessUnitId.Should().Be.EqualTo(item.BusinessUnitId);
				itemToCompare.DayOffName.Should().Be.EqualTo(item.DayOffName);
				itemToCompare.DayOffShortname.Should().Be.EqualTo(item.DayOffShortname);
				itemToCompare.DatasourceUpdateDate.Should().Be.EqualTo(item.DatasourceUpdateDate);
				itemToCompare.DisplayColor.Should().Be.EqualTo(item.DisplayColor);
				itemToCompare.DisplayColorHtml.Should().Be.EqualTo(item.DisplayColorHtml);
			});
		}

		[Test]
		public void ShouldUpdateDayOffWithCodeNull()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var dayOffCode = Guid.NewGuid();
			analyticsDataFactory.Setup(new DimDayOff(1, null, "DayOff", _datasource, businessUnitId));
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DayOffs().Count.Should().Be.EqualTo(2);
				var item = new AnalyticsDayOff
				{
					DayOffCode = dayOffCode,
					BusinessUnitId = businessUnitId,
					DayOffName = "DayOff",
					DayOffShortname = "DO",
					DatasourceUpdateDate = DateTime.Today,
					DisplayColor = -1,
					DisplayColorHtml = "#808080"
				};
				Target.AddOrUpdate(item);
				Target.DayOffs().Count.Should().Be.EqualTo(2);
				var itemToCompare = Target.DayOffs().First(a => a.DayOffCode == dayOffCode);
				itemToCompare.BusinessUnitId.Should().Be.EqualTo(item.BusinessUnitId);
				itemToCompare.DayOffName.Should().Be.EqualTo(item.DayOffName);
				itemToCompare.DayOffShortname.Should().Be.EqualTo(item.DayOffShortname);
				itemToCompare.DatasourceUpdateDate.Should().Be.EqualTo(item.DatasourceUpdateDate);
				itemToCompare.DisplayColor.Should().Be.EqualTo(item.DisplayColor);
				itemToCompare.DisplayColorHtml.Should().Be.EqualTo(item.DisplayColorHtml);
			});
		}
	}
}