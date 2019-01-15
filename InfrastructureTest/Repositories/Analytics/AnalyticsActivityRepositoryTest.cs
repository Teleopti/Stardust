using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsActivityRepositoryTest
	{
		public IAnalyticsActivityRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
		}
		
		[Test]
		public void ShouldFindActivityId()
		{
			var code = Guid.NewGuid();
			var activity = new Activity(10, code, "Activity name", Color.AliceBlue, new ExistingDatasources(new UtcAndCetTimeZones()), 4);
			analyticsDataFactory.Setup(activity);
			analyticsDataFactory.Persist();

			var act = WithAnalyticsUnitOfWork.Get(() => Target.Activity(code));
			act.ActivityId.Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldFindActivities()
		{
			var businessUnit = new BusinessUnit(ServiceLocatorForEntity.CurrentBusinessUnit.Current(), 1,4);
			var code = Guid.NewGuid();
			var activity = new Activity(10, code, "Activity name", Color.AliceBlue, new ExistingDatasources(new UtcAndCetTimeZones()), 4);
			analyticsDataFactory.Setup(activity);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();
			
			var act = WithAnalyticsUnitOfWork.Get(() => Target.Activities()).Single();
			act.ActivityId.Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldHandleActivityCodeNotFound()
		{
			var act = WithAnalyticsUnitOfWork.Get(() => Target.Activity(Guid.Empty));
			act.Should().Be.Null();
		}

		[Test]
		public void AddActivity1()
		{
			var code = Guid.NewGuid();
			var analyticsActivity = new AnalyticsActivity
			{
				ActivityCode = code,
				ActivityName = "Activityname 123",
				BusinessUnitId = 321,
				DisplayColor = 1234,
				DisplayColorHtml = "#123456",
				InReadyTime = true,
				InContractTime = true,
				InPaidTime = true,
				InWorkTime = true,
				IsDeleted = true,
				DatasourceId = 123,
				DatasourceUpdateDate = new DateTime(2010, 1, 1, 12, 0, 0)
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddActivity(analyticsActivity));
			var addedAnalyticsActivity = WithAnalyticsUnitOfWork.Get(() => Target.Activity(code));

			assertActivity(analyticsActivity, addedAnalyticsActivity);
		}

		[Test]
		public void AddActivity2()
		{
			var code = Guid.NewGuid();
			var analyticsActivity = new AnalyticsActivity
			{
				ActivityCode = code,
				ActivityName = "Activityname 123",
				BusinessUnitId = 321,
				DisplayColor = 1234,
				DisplayColorHtml = "#123456",
				InReadyTime = false,
				InContractTime = false,
				InPaidTime = false,
				InWorkTime = false,
				IsDeleted = false,
				DatasourceId = 123,
				DatasourceUpdateDate = new DateTime(2012, 1, 1, 12, 0, 0)
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddActivity(analyticsActivity));
			var addedAnalyticsActivity = WithAnalyticsUnitOfWork.Get(() => Target.Activity(code));

			assertActivity(analyticsActivity, addedAnalyticsActivity);
		}

		[Test]
		public void UpdateActivity()
		{
			var code = Guid.NewGuid();
			var analyticsActivity = new AnalyticsActivity
			{
				ActivityCode = code,
				ActivityName = "Activityname 123",
				BusinessUnitId = 321,
				DisplayColor = 1234,
				DisplayColorHtml = "#123456",
				InReadyTime = false,
				InContractTime = false,
				InPaidTime = false,
				InWorkTime = false,
				IsDeleted = false,
				DatasourceId = 123,
				DatasourceUpdateDate = new DateTime(2012, 1, 1, 12, 0, 0)
			};

			// Add
			WithAnalyticsUnitOfWork.Do(() => Target.AddActivity(analyticsActivity));
			
			// Update
			analyticsActivity.ActivityName = "New activity name";
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateActivity(analyticsActivity));

			// Assert
			var updatedAnalyticsActivity = WithAnalyticsUnitOfWork.Get(() => Target.Activity(code));
			assertActivity(analyticsActivity, updatedAnalyticsActivity);
		}

		private static void assertActivity(AnalyticsActivity analyticsActivity, AnalyticsActivity addedAnalyticsActivity)
		{
			addedAnalyticsActivity.ActivityCode.Should().Be.EqualTo(analyticsActivity.ActivityCode);
			addedAnalyticsActivity.ActivityName.Should().Be.EqualTo(analyticsActivity.ActivityName);
			addedAnalyticsActivity.BusinessUnitId.Should().Be.EqualTo(analyticsActivity.BusinessUnitId);
			addedAnalyticsActivity.DisplayColor.Should().Be.EqualTo(analyticsActivity.DisplayColor);
			addedAnalyticsActivity.DisplayColorHtml.Should().Be.EqualTo(analyticsActivity.DisplayColorHtml);
			addedAnalyticsActivity.InReadyTime.Should().Be.EqualTo(analyticsActivity.InReadyTime);
			addedAnalyticsActivity.InWorkTime.Should().Be.EqualTo(analyticsActivity.InWorkTime);
			addedAnalyticsActivity.InContractTime.Should().Be.EqualTo(analyticsActivity.InContractTime);
			addedAnalyticsActivity.InPaidTime.Should().Be.EqualTo(analyticsActivity.InPaidTime);
			addedAnalyticsActivity.IsDeleted.Should().Be.EqualTo(analyticsActivity.IsDeleted);
			addedAnalyticsActivity.DatasourceId.Should().Be.EqualTo(analyticsActivity.DatasourceId);
			addedAnalyticsActivity.DatasourceUpdateDate.Should().Be.EqualTo(analyticsActivity.DatasourceUpdateDate);

			addedAnalyticsActivity.InContractTimeName.Should().Be
				.EqualTo((analyticsActivity.InContractTime ? "" : "Not ") + "In Contract Time");
			addedAnalyticsActivity.InPaidTimeName.Should().Be
				.EqualTo((analyticsActivity.InPaidTime ? "" : "Not ") + "In Paid Time");
			addedAnalyticsActivity.InReadyTimeName.Should().Be
				.EqualTo((analyticsActivity.InReadyTime ? "" : "Not ") + "In Ready Time");
			addedAnalyticsActivity.InWorkTimeName.Should().Be
				.EqualTo((analyticsActivity.InWorkTime ? "" : "Not ") + "In Work Time");
		}
	}
}