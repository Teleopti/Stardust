using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("BucketB")]
	[TestFixture]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsGroupPageRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsGroupPageRepository Target;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldGetGroupPage()
		{
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
			Target.AddGroupPageIfNotExisting(groupPage);

			var result = Target.GetGroupPage(groupPage.GroupPageCode, groupPage.BusinessUnitCode).First();
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
			Target.AddGroupPageIfNotExisting(groupPage);
			Target.AddGroupPageIfNotExisting(groupPage);
			var result = Target.GetGroupPageByGroupCode(groupPage.GroupCode, groupPage.BusinessUnitCode);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetGroupPageByGroupCode()
		{
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
			Target.AddGroupPageIfNotExisting(groupPage);

			var result = Target.GetGroupPageByGroupCode(groupPage.GroupCode, groupPage.BusinessUnitCode);
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
			Target.AddGroupPageIfNotExisting(groupPage);

			groupPage.GroupPageName = "GroupPageName2";

			Target.UpdateGroupPage(groupPage);

			var result = Target.GetGroupPage(groupPage.GroupPageCode, groupPage.BusinessUnitCode).First();
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
			Target.AddGroupPageIfNotExisting(groupPage);
			Target.DeleteGroupPages(new[] { groupPage.GroupPageCode }, groupPage.BusinessUnitCode);

			Target.GetGroupPage(groupPage.GroupPageCode, groupPage.BusinessUnitCode).Should().Be.Empty();
		}

		[Test]
		public void ShouldBeAbleToDeleteBasedOnGroupCode()
		{
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
			Target.AddGroupPageIfNotExisting(groupPage);
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
			Target.AddGroupPageIfNotExisting(groupPage2);

			Target.DeleteGroupPagesByGroupCodes(new [] { groupPage.GroupCode}, groupPage.BusinessUnitCode);

			var result = Target.GetGroupPageByGroupCode(groupPage.GroupCode, groupPage.BusinessUnitCode);
			var result2 = Target.GetGroupPageByGroupCode(groupPage2.GroupCode, groupPage2.BusinessUnitCode);
			result.Should().Be.Null();
			result2.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBeAbleToGetListOfBuildInGroupPages()
		{
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
			Target.AddGroupPageIfNotExisting(groupPage);
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
			Target.AddGroupPageIfNotExisting(groupPage2);

			var result = Target.GetBuildInGroupPageBase(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()).FirstOrDefault();

			result.Should().Not.Be.Null();
			result.GroupPageName.Should().Be.EqualTo(groupPage.GroupPageName);
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
		}
	}
}