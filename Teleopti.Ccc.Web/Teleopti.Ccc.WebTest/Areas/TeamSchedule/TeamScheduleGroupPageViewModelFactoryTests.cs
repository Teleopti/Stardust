using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.Security;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.WebTest.Areas.Anywhere;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture]
	public class TeamScheduleGroupPageViewModelFactoryTests
	{
		private TeamScheduleGroupPageViewModelFactory _factory;

		[Test]
		public void GroupViewModelFactoryShouldNotBeNull()
		{
			setUpFactory(null, null);
			_factory.Should().Not.Be(null);
		}

		[Test]
		public void ShouldCreateGroupViewModel()
		{
			var groupPage = new ReadOnlyGroupPage
			{
				PageId = Group.PageMainId,
				PageName = "xxMain"
			};

			var firstGroup = new ReadOnlyGroupDetail
			{
				PageId = Group.PageMainId,
				GroupId = Guid.NewGuid(),
				GroupName = "Team green"
			};

			setUpFactory(new[] {groupPage}, new[] {firstGroup});
			var date = new DateOnly(2015, 01, 01);
			var vm = _factory.GetBusinessHierarchyPageGroupViewModelsByDate(date);

			vm.Count().Should().Be(1);
			vm.First().Id.Should().Be(firstGroup.GroupId);
			vm.First().Name.Should().Be(firstGroup.GroupName);
		}

		[Test]
		public void ShouldReturnEmptyArrayWhenNoGroupPage()
		{
			setUpFactory(new List<ReadOnlyGroupPage>(), new List<ReadOnlyGroupDetail>());
			var date = new DateOnly(2015, 01, 01);
			var vm = _factory.GetBusinessHierarchyPageGroupViewModelsByDate(date);

			vm.Count().Should().Be(0);
		}

		private void setUpFactory(IList<ReadOnlyGroupPage> readOnlyGroupPages, IList<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			var groupPageRepo = new FakeGroupingReadOnlyRepository(readOnlyGroupPages, readOnlyGroupDetails);
			_factory = new TeamScheduleGroupPageViewModelFactory(groupPageRepo);
		}
	}
}