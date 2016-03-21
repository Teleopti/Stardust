using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsGroupPageRepositoryTest
	{
		ICurrentDataSource currentDataSource;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();
			currentDataSource = CurrentDataSource.Make();
		}

		[Test]
		public void ShouldGetGroupPage()
		{
			var target = new AnalyticsGroupPageRepository(currentDataSource);
			var groupPage = new AnalyticsGroupPage
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPage(groupPage);

			var result = target.GetGroupPage(groupPage.GroupPageCode).First();
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageName.Should().Be.EqualTo(groupPage.GroupPageName);
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
			result.GroupCode.Should().Be.EqualTo(groupPage.GroupCode);
			result.GroupName.Should().Be.EqualTo(groupPage.GroupName);
			result.GroupIsCustom.Should().Be.EqualTo(groupPage.GroupIsCustom);
			result.BusinessUnitCode.Should().Be.EqualTo(groupPage.BusinessUnitCode);
		}

		[Test]
		public void ShouldUpdateGroupPage()
		{
			var target = new AnalyticsGroupPageRepository(currentDataSource);
			var groupPage = new AnalyticsGroupPage
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPage(groupPage);

			groupPage.GroupPageName = "GroupPageName2";

			target.UpdateGroupPage(groupPage);

			var result = target.GetGroupPage(groupPage.GroupPageCode).First();
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageName.Should().Be.EqualTo("GroupPageName2");
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
			result.GroupCode.Should().Be.EqualTo(groupPage.GroupCode);
			result.GroupName.Should().Be.EqualTo(groupPage.GroupName);
			result.GroupIsCustom.Should().Be.EqualTo(groupPage.GroupIsCustom);
			result.BusinessUnitCode.Should().Be.EqualTo(groupPage.BusinessUnitCode);
		}

		[Test]
		public void ShouldDeleteGroupPage()
		{
			var target = new AnalyticsGroupPageRepository(currentDataSource);
			var groupPage = new AnalyticsGroupPage
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPage(groupPage);
			target.DeleteGroupPages(new[] { groupPage.GroupPageCode });

			target.GetGroupPage(groupPage.GroupPageCode).Should().Be.Empty();
		}
	}
}