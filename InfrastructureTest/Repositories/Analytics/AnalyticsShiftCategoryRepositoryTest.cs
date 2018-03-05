using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsShiftCategoryRepositoryTest
	{
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public IAnalyticsShiftCategoryRepository Target;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		[Test]
		public void ShouldLoadShiftCategories()
		{
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(1, Guid.NewGuid()));
			analyticsDataFactory.Setup(new ShiftCategory(1, Guid.NewGuid(), "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Persist();

			var cats = WithAnalyticsUnitOfWork.Get(() => Target.ShiftCategories());
			cats.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadShiftCategory()
		{
			var shiftCategoryId = Guid.NewGuid();
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(1, Guid.NewGuid()));
			var analyticsShiftCategory = new ShiftCategory(1, shiftCategoryId, "Kattegat", Color.Green, _datasource, businessUnitId);
			analyticsDataFactory.Setup(analyticsShiftCategory);
			analyticsDataFactory.Persist();

			var shiftCategoryFromDb = WithAnalyticsUnitOfWork.Get(() => Target.ShiftCategory(shiftCategoryId));
			shiftCategoryFromDb.ShiftCategoryCode.Should().Be.EqualTo(shiftCategoryId);
			shiftCategoryFromDb.ShiftCategoryName.Should().Be.EqualTo("Kattegat");
			shiftCategoryFromDb.BusinessUnitId.Should().Be.EqualTo(businessUnitId);
			shiftCategoryFromDb.DisplayColor.Should().Be.EqualTo(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldReturnNullShiftCategoryWhenNotFound()
		{
			var shiftCategoryId = Guid.NewGuid();
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(1, Guid.NewGuid()));
			var analyticsShiftCategory = new ShiftCategory(1, shiftCategoryId, "Kattegat", Color.Green, _datasource, businessUnitId);
			analyticsDataFactory.Setup(analyticsShiftCategory);
			analyticsDataFactory.Persist();

			var shiftCategoryFromDb = WithAnalyticsUnitOfWork.Get(() => Target.ShiftCategory(Guid.NewGuid()));
			shiftCategoryFromDb.Should().Be.Null();
		}

		[Test]
		public void ShouldAddShiftCategoryAndMapAllValues()
		{
			var analyticsShiftCategory = new AnalyticsShiftCategory
			{
				ShiftCategoryCode = Guid.NewGuid(),
				ShiftCategoryName = "Shift Category Name",
				ShiftCategoryShortname = "Shift Category Short Name",
				BusinessUnitId = -1,
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1),
				DisplayColor = 123,
				IsDeleted = false
			};

			WithAnalyticsUnitOfWork.Do(() => Target.AddShiftCategory(analyticsShiftCategory));
			WithAnalyticsUnitOfWork.Do(() => Target.ShiftCategories().Count.Should().Be.EqualTo(1));
			var shiftCategoryFromDb = WithAnalyticsUnitOfWork.Get(() => Target.ShiftCategories().First(a => a.ShiftCategoryCode == analyticsShiftCategory.ShiftCategoryCode));
			shiftCategoryFromDb.ShiftCategoryCode.Should().Be.EqualTo(analyticsShiftCategory.ShiftCategoryCode);
			shiftCategoryFromDb.ShiftCategoryName.Should().Be.EqualTo(analyticsShiftCategory.ShiftCategoryName);
			shiftCategoryFromDb.ShiftCategoryShortname.Should().Be.EqualTo(analyticsShiftCategory.ShiftCategoryShortname);
			shiftCategoryFromDb.BusinessUnitId.Should().Be.EqualTo(analyticsShiftCategory.BusinessUnitId);
			shiftCategoryFromDb.DatasourceId.Should().Be.EqualTo(analyticsShiftCategory.DatasourceId);
			shiftCategoryFromDb.DatasourceUpdateDate.Should().Be.EqualTo(analyticsShiftCategory.DatasourceUpdateDate);
			shiftCategoryFromDb.DisplayColor.Should().Be.EqualTo(analyticsShiftCategory.DisplayColor);
			shiftCategoryFromDb.IsDeleted.Should().Be.EqualTo(analyticsShiftCategory.IsDeleted);
		}

		[Test]
		public void ShouldUpdateShiftCategoryAndMapAllValues()
		{
			var shiftCategoryCode = Guid.NewGuid();
			analyticsDataFactory.Setup(new ShiftCategory(1, shiftCategoryCode, "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Persist();
			WithAnalyticsUnitOfWork.Do(() => Target.ShiftCategories().Count.Should().Be.EqualTo(1));

			var analyticsShiftCategory = new AnalyticsShiftCategory
			{
				ShiftCategoryCode = shiftCategoryCode,
				ShiftCategoryName = "New Shift Category Name",
				ShiftCategoryShortname = "New Shift Category Short Name",
				BusinessUnitId = businessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1),
				DisplayColor = 123,
				IsDeleted = false
			};

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateShiftCategory(analyticsShiftCategory));
			WithAnalyticsUnitOfWork.Do(() => Target.ShiftCategories().Count.Should().Be.EqualTo(1));
			var shiftCategoryFromDb = WithAnalyticsUnitOfWork.Get(() => Target.ShiftCategories()).First(a => a.ShiftCategoryCode == shiftCategoryCode);
			shiftCategoryFromDb.ShiftCategoryCode.Should().Be.EqualTo(analyticsShiftCategory.ShiftCategoryCode);
			shiftCategoryFromDb.ShiftCategoryName.Should().Be.EqualTo(analyticsShiftCategory.ShiftCategoryName);
			shiftCategoryFromDb.ShiftCategoryShortname.Should().Be.EqualTo(analyticsShiftCategory.ShiftCategoryShortname);
			shiftCategoryFromDb.BusinessUnitId.Should().Be.EqualTo(analyticsShiftCategory.BusinessUnitId);
			shiftCategoryFromDb.DatasourceId.Should().Be.EqualTo(analyticsShiftCategory.DatasourceId);
			shiftCategoryFromDb.DatasourceUpdateDate.Should().Be.EqualTo(analyticsShiftCategory.DatasourceUpdateDate);
			shiftCategoryFromDb.DisplayColor.Should().Be.EqualTo(analyticsShiftCategory.DisplayColor);
			shiftCategoryFromDb.IsDeleted.Should().Be.EqualTo(analyticsShiftCategory.IsDeleted);
		}
	}
}