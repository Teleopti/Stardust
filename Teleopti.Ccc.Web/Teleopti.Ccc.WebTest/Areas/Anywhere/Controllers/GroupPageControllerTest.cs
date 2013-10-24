
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	public class GroupPageControllerTest
	{
		[Test]
		public void ShouldReturnAvailableGroupPages()
		{
			var groupPage = new ReadOnlyGroupPage
				{
					PageName = "Skill"
				};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { groupPage }, new List<ReadOnlyGroupDetail>()), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object)gp.Name).Should().Be.EqualTo(groupPage.PageName);
		}

		[Test]
		public void ShouldReturnMyTeamAsDefaultGroup()
		{
			var site = SiteFactory.CreateSimpleSite(" ");
			var team = new Team { Site = site, Description = new Description("Team red")};
			team.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today, team);
			var loggedOnUser = new FakeLoggedOnUser(person);

			var firstGroup = new ReadOnlyGroupDetail
				{
					GroupId = Guid.Empty,
					GroupName = "Team green"
				};

			var secondGroup = new ReadOnlyGroupDetail
				{
					GroupId = team.Id.Value,
					GroupName = team.Description.Name
				};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { new ReadOnlyGroupPage { PageName = "" } }, new []{ firstGroup, secondGroup }), loggedOnUser);

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result;
			((object)gp.SelectedGroupId).Should().Be.EqualTo(secondGroup.GroupId);
		}

		[Test]
		public void ShouldReturnNoDefaultGroupIfIHaveNoTeam()
		{
			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { new ReadOnlyGroupPage { PageName = "" } }, new []{ new ReadOnlyGroupDetail() }), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result;
			((object) gp.SelectedGroupId).Should().Be.Null();
		}

		[Test]
		public void ShouldReplaceGroupPageNameFromResourceMangerIfItStartsWithXx()
		{
			var groupPage = new ReadOnlyGroupPage
				{
					PageName = "xxMain"
				};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { groupPage }, new List<ReadOnlyGroupDetail>()), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object)gp.Name).Should().Be.EqualTo(Resources.ResourceManager.GetString(groupPage.PageName.Substring(2)));
		}

		[Test]
		public void ShouldReturnGroupNameWithGroupPageName()
		{
			var groupPage = new ReadOnlyGroupPage
				{
				PageName = "A group"
			};

			var firstGroup = new ReadOnlyGroupDetail
				{
				GroupId = Guid.Empty,
				GroupName = "Team green"
			};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { groupPage }, new []{ firstGroup }), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object) gp.Groups[0].Name).Should().Be.EqualTo(groupPage.PageName + "/" + firstGroup.GroupName);
		}

		[Test]
		public void ShouldReturnGroupNameWithoutGroupPageNameForBusinessHierarchy()
		{
			var groupPage = new ReadOnlyGroupPage
				{
				PageId = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF"),
				PageName = "xxMain"
			};

			var firstGroup = new ReadOnlyGroupDetail
				{
				GroupId = Guid.Empty,
				GroupName = "Team green"
			};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { groupPage }, new []{ firstGroup }), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object)gp.Groups[0].Name).Should().Be.EqualTo(firstGroup.GroupName);
		}

		[Test]
		public void ShouldOrderBuildInGroupPageByName()
		{
			var groupPage1 = new ReadOnlyGroupPage
			{
				PageName = "xxContract"
			};
			var groupPage2 = new ReadOnlyGroupPage
			{
				PageId = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF"),
				PageName = "xxMain"
			};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { groupPage1, groupPage2 }, new List<ReadOnlyGroupDetail>()), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;
			(result.GroupPages[0].Name as string).Should().Be.EqualTo(Resources.ResourceManager.GetString(groupPage2.PageName.Substring(2)));
			(result.GroupPages[1].Name as string).Should().Be.EqualTo(Resources.ResourceManager.GetString(groupPage1.PageName.Substring(2)));
		}

		[Test]
		public void ShouldOrderCustomGroupPageByName()
		{
			var groupPage1 = new ReadOnlyGroupPage
			{
				PageName = "B group Page"
			};
			var groupPage2 = new ReadOnlyGroupPage
			{
				PageName = "A group Page"
			};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { groupPage1, groupPage2 }, new List<ReadOnlyGroupDetail>()), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;
			(result.GroupPages[0].Name as string).Should().Be.EqualTo(groupPage2.PageName);
			(result.GroupPages[1].Name as string).Should().Be.EqualTo(groupPage1.PageName);
		}

		[Test]
		public void ShouldOrderCustomGroupPageAfterBuildInGroupPage()
		{
			var groupPage1 = new ReadOnlyGroupPage
			{
				PageName = "A group Page"
			};
			var groupPage2 = new ReadOnlyGroupPage
			{
				PageName = "xxContract"
			};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] { groupPage1, groupPage2 }, new List<ReadOnlyGroupDetail>()), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;
			(result.GroupPages[0].Name as string).Should().Be.EqualTo(Resources.ResourceManager.GetString(groupPage2.PageName.Substring(2)));
			(result.GroupPages[1].Name as string).Should().Be.EqualTo(groupPage1.PageName);
		}

	}

	public class FakeGroupingReadOnlyRepository : IGroupingReadOnlyRepository
	{
		private readonly IList<ReadOnlyGroupPage> ReadOnlyGroupPages;
		private readonly IList<ReadOnlyGroupDetail> ReadOnlyGroupDetails;

		public FakeGroupingReadOnlyRepository(IList<ReadOnlyGroupPage> readOnlyGroupPages, IList<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			ReadOnlyGroupPages = readOnlyGroupPages;
			ReadOnlyGroupDetails = readOnlyGroupDetails;
		}

		public IEnumerable<ReadOnlyGroupPage> AvailableGroupPages()
		{
			return ReadOnlyGroupPages;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnly queryDate)
		{
			return ReadOnlyGroupDetails;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnly queryDate)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnly queryDate)
		{
			throw new NotImplementedException();
		}

		public void UpdateGroupingReadModel(ICollection<Guid> inputIds)
		{
			throw new NotImplementedException();
		}

		public void UpdateGroupingReadModelGroupPage(ICollection<Guid> inputIds)
		{
			throw new NotImplementedException();
		}

		public void UpdateGroupingReadModelData(ICollection<Guid> inputIds)
		{
			throw new NotImplementedException();
		}
	}
}