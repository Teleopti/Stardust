using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
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
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage);

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
		public void ShouldNotAddIfExisting()
		{
			var target = new AnalyticsGroupPageRepository(currentDataSource);
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage);
			target.AddGroupPageIfNotExisting(groupPage);
			var result = target.GetGroupPageByGroupCode(groupPage.GroupCode);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetGroupPageByGroupCode()
		{
			var target = new AnalyticsGroupPageRepository(currentDataSource);
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage);

			var result = target.GetGroupPageByGroupCode(groupPage.GroupCode);
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
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage);

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
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage);
			target.DeleteGroupPages(new[] { groupPage.GroupPageCode });

			target.GetGroupPage(groupPage.GroupPageCode).Should().Be.Empty();
		}

		[Test]
		public void ShouldBeAbleToDeleteBasedOnGroupCode()
		{
			var target = new AnalyticsGroupPageRepository(currentDataSource);
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage);
			var groupPage2 = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName2",
				GroupPageNameResourceKey = "GroupPageNameResourceKey2",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName2",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage2);

			target.DeleteGroupPagesByGroupCodes(new [] { groupPage.GroupCode});

			var result = target.GetGroupPageByGroupCode(groupPage.GroupCode);
			var result2 = target.GetGroupPageByGroupCode(groupPage2.GroupCode);
			result.Should().Be.Null();
			result2.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBeAbleToGetListOfBuildInGroupPages()
		{
			var target = new AnalyticsGroupPageRepository(currentDataSource);
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage);
			var groupPage2 = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName2",
				GroupPageNameResourceKey = null,
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName2",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			target.AddGroupPageIfNotExisting(groupPage2);

			var result = target.GetBuildInGroupPageBase().FirstOrDefault();

			result.Should().Not.Be.Null();
			result.GroupPageName.Should().Be.EqualTo(groupPage.GroupPageName);
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
		}
	}
}