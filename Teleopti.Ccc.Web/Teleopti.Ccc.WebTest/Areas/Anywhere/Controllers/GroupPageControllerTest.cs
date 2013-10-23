
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	public class GroupPageControllerTest
	{
		[Test]
		public void ShouldReturnAvailableGroupPages()
		{
			var groupPage = new ReadOnlyGroupPage()
				{
					PageName = "Skill"
				};
			var group = new ReadOnlyGroupDetail()
				{
					GroupId = Guid.Empty,
					GroupName = "Phone"
				};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(groupPage, new List<ReadOnlyGroupDetail>(){ group }), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result.GroupPages[0];
			((object)gp.Name).Should().Be.EqualTo(groupPage.PageName);
			dynamic g = gp.Groups[0];
			((object)g.Id).Should().Be.EqualTo(group.GroupId);
			((object)g.Name).Should().Be.EqualTo(group.GroupName);
		}

		[Test]
		public void ShouldReturnMyTeamAsDefaultGroup()
		{
			var site = SiteFactory.CreateSimpleSite(" ");
			var team = new Team() { Site = site, Description = new Description("Team red")};
			team.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today, team);
			var loggedOnUser = new FakeLoggedOnUser(person);

			var firstGroup = new ReadOnlyGroupDetail()
				{
					GroupId = Guid.Empty,
					GroupName = "Team green"
				};

			var secondGroup = new ReadOnlyGroupDetail()
				{
					GroupId = team.Id.Value,
					GroupName = team.Description.Name
				};

			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new ReadOnlyGroupPage(), new List<ReadOnlyGroupDetail>() { firstGroup, secondGroup }), loggedOnUser);

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result;
			((object)gp.SelectedGroupId).Should().Be.EqualTo(secondGroup.GroupId);
		}

		[Test]
		public void ShouldReturnNoDefaultGroupIfIHaveNoTeam()
		{
			var target = new GroupPageController(new FakeGroupingReadOnlyRepository(new ReadOnlyGroupPage(), new List<ReadOnlyGroupDetail>() { new ReadOnlyGroupDetail() }), new FakeLoggedOnUser());

			dynamic result = target.AvailableGroupPages(DateTime.Now).Data;

			dynamic gp = result;
			((object) gp.SelectedGroupId).Should().Be.Null();
		}
	}

	public class FakeGroupingReadOnlyRepository : IGroupingReadOnlyRepository
	{
		public IList<ReadOnlyGroupPage> ReadOnlyGroupPages = new List<ReadOnlyGroupPage>();
		public IList<ReadOnlyGroupDetail> ReadOnlyGroupDetails;

		public FakeGroupingReadOnlyRepository(ReadOnlyGroupPage readOnlyGroupPage, List<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			ReadOnlyGroupPages.Add(readOnlyGroupPage);
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