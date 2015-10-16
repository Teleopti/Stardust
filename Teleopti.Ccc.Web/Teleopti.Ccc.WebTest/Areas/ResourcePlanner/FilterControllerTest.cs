using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.WebTest.Areas.Anywhere;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class FilterControllerTest
	{
		[Test]
		public void ShouldGetPermittedFilters()
		{
			var pageId = Guid.NewGuid();
			
			var target =
				new FilterController(
					new FakeGroupingReadOnlyRepository(
						new List<ReadOnlyGroupPage> {new ReadOnlyGroupPage {PageId = pageId, PageName = "Sysselsättningsgrad"}},
						new List<ReadOnlyGroupDetail>
						{
							new ReadOnlyGroupDetail {PageId = pageId, GroupId = Guid.NewGuid(), GroupName = "100%"}
						}),new FakePermissionProvider());

			var result = target.GetFilters();
			var single = result.Single();
			single.Name.Should().Be.EqualTo("Sysselsättningsgrad");
			single.Id.Should().Be.EqualTo(pageId);
			single.Items.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotGetUnpermittedFilters()
		{
			var pageId = Guid.NewGuid();

			var target =
				new FilterController(
					new FakeGroupingReadOnlyRepository(
						new List<ReadOnlyGroupPage> { new ReadOnlyGroupPage { PageId = pageId, PageName = "Sysselsättningsgrad" } },
						new List<ReadOnlyGroupDetail>
						{
							new ReadOnlyGroupDetail {PageId = pageId, GroupId = Guid.NewGuid(), GroupName = "100%"}
						}), new FakeNoPermissionProvider());

			var result = target.GetFilters();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetTranslatedNameForPage()
		{
			var pageId = Guid.NewGuid();

			var target =
				new FilterController(
					new FakeGroupingReadOnlyRepository(
						new List<ReadOnlyGroupPage> { new ReadOnlyGroupPage { PageId = pageId, PageName = "xxMain" } },
						new List<ReadOnlyGroupDetail>
						{
							new ReadOnlyGroupDetail {PageId = pageId, GroupId = Guid.NewGuid(), GroupName = "Paris/Team 1"}
						}), new FakePermissionProvider());

			var result = target.GetFilters();
			var single = result.Single();
			single.Name.Should().Not.Be.EqualTo("xxMain");
		}
	}
}
