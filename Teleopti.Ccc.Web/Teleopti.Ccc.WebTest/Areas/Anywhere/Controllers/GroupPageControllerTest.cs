using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
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

			var target =
				new GroupPageController(new FakeGroupingReadOnlyRepository(new[] {groupPage}, new List<ReadOnlyGroupDetail>()),
					new FakeLoggedOnUser(), new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object) gp.Name).Should().Be.EqualTo(groupPage.PageName);
		}

		[Test]
		public void ShouldReturnMyTeamAsDefaultGroup()
		{
			var site = SiteFactory.CreateSimpleSite("s");

			var teamId = Guid.NewGuid();
			var team = new Team {Site = site, Description = new Description("Team red")};
			team.SetId(teamId);

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today, team);
			var loggedOnUser = new FakeLoggedOnUser(person);

			var firstGroup = new ReadOnlyGroupDetail
			{
				GroupId = Guid.Empty,
				GroupName = "Team green"
			};

			var secondGroup = new ReadOnlyGroupDetail
			{
				GroupId = teamId,
				GroupName = team.Description.Name
			};

			var target =
				new GroupPageController(
					new FakeGroupingReadOnlyRepository(new[] {new ReadOnlyGroupPage {PageName = ""}}, new[] {firstGroup, secondGroup}),
					loggedOnUser, new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result;
			((object) gp.DefaultGroupId).Should().Be.EqualTo(secondGroup.GroupId);
		}

		[Test]
		public void ShouldReturnNoDefaultGroupIfIHaveNoTeam()
		{
			var target =
				new GroupPageController(
					new FakeGroupingReadOnlyRepository(new[] {new ReadOnlyGroupPage {PageName = ""}}, new[] {new ReadOnlyGroupDetail()}),
					new FakeLoggedOnUser(), new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result;
			((object) gp.DefaultGroupId).Should().Be.Null();
		}

		[Test]
		public void ShouldReplaceGroupPageNameFromResourceMangerIfItStartsWithXx()
		{
			const string pageName = "xxMain";
			var groupPage = new ReadOnlyGroupPage
			{
				PageName = pageName
			};

			var target =
				new GroupPageController(new FakeGroupingReadOnlyRepository(new[] {groupPage}, new List<ReadOnlyGroupDetail>()),
					new FakeLoggedOnUser(), new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object) gp.Name).Should().Be.EqualTo(Resources.ResourceManager.GetString(pageName.Substring(2)));
		}

		[Test]
		public void ShouldReturnGroupNameWithGroupPageName()
		{
			var pageId = Guid.NewGuid();
			var groupPage = new ReadOnlyGroupPage
			{
				PageId = pageId,
				PageName = "A group"
			};

			var firstGroup = new ReadOnlyGroupDetail
			{
				PageId = pageId,
				GroupId = Guid.Empty,
				GroupName = "Team green"
			};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] {groupPage}, new[] {firstGroup}),
				new FakeLoggedOnUser(), new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object) gp.Groups[0].Name).Should().Be.EqualTo(groupPage.PageName + "/" + firstGroup.GroupName);
		}

		[Test]
		public void ShouldReturnGroupNameWithoutGroupPageNameForBusinessHierarchy()
		{
			var groupPage = new ReadOnlyGroupPage
			{
				PageId = Group.PageMainId,
				PageName = "xxMain"
			};

			var firstGroup = new ReadOnlyGroupDetail
			{
				PageId = Group.PageMainId,
				GroupId = Guid.Empty,
				GroupName = "Team green"
			};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new[] {groupPage}, new[] {firstGroup}),
				new FakeLoggedOnUser(), new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object) gp.Groups[0].Name).Should().Be.EqualTo(firstGroup.GroupName);
		}

		[Test]
		public void ShouldOrderBuildInGroupPageByName()
		{
			const string pageName1 = "xxContract";
			var groupPage1 = new ReadOnlyGroupPage
			{
				PageName = pageName1
			};
			const string pageName2 = "xxMain";
			var groupPage2 = new ReadOnlyGroupPage
			{
				PageId = Group.PageMainId,
				PageName = pageName2
			};

			var target =
				new GroupPageController(
					new FakeGroupingReadOnlyRepository(new[] {groupPage1, groupPage2}, new List<ReadOnlyGroupDetail>()),
					new FakeLoggedOnUser(), new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;
			(result.GroupPages[0].Name as string).Should()
				.Be.EqualTo(Resources.ResourceManager.GetString(pageName2.Substring(2)));
			(result.GroupPages[1].Name as string).Should()
				.Be.EqualTo(Resources.ResourceManager.GetString(pageName1.Substring(2)));
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

			var target =
				new GroupPageController(
					new FakeGroupingReadOnlyRepository(new[] {groupPage1, groupPage2}, new List<ReadOnlyGroupDetail>()),
					new FakeLoggedOnUser(), new UserTextTranslator());

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
			const string pageName2 = "xxContract";
			var groupPage2 = new ReadOnlyGroupPage
			{
				PageName = pageName2
			};

			var target =
				new GroupPageController(
					new FakeGroupingReadOnlyRepository(new[] {groupPage1, groupPage2}, new List<ReadOnlyGroupDetail>()),
					new FakeLoggedOnUser(), new UserTextTranslator());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;
			(result.GroupPages[0].Name as string).Should()
				.Be.EqualTo(Resources.ResourceManager.GetString(pageName2.Substring(2)));
			(result.GroupPages[1].Name as string).Should().Be.EqualTo(groupPage1.PageName);
		}

	}

	public class FakeGroupingReadOnlyRepository : IGroupingReadOnlyRepository
	{
		private readonly IList<ReadOnlyGroupPage> _readOnlyGroupPages;
		private readonly IList<ReadOnlyGroupDetail> _readOnlyGroupDetails;

		public FakeGroupingReadOnlyRepository(IList<ReadOnlyGroupPage> readOnlyGroupPages,
			IList<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			_readOnlyGroupPages = readOnlyGroupPages;
			_readOnlyGroupDetails = readOnlyGroupDetails;
		}

		public IEnumerable<ReadOnlyGroupPage> AvailableGroupPages()
		{
			return _readOnlyGroupPages;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(IEnumerable<ReadOnlyGroupPage> groupPages, DateOnly queryDate)
		{
			return _readOnlyGroupDetails;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnly queryDate)
		{
			return _readOnlyGroupDetails;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnly queryDate)
		{
			return _readOnlyGroupDetails;
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
