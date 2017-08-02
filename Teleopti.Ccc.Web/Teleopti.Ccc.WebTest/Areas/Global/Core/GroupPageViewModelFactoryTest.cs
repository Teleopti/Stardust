using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global.Core
{

	[TestFixture,DomainTest]
	public class GroupPageViewModelFactoryTest:ISetup
	{
		public GroupPageViewModelFactory Target;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakeUserUiCulture UserCulture;
		public FakePermissionProvider PermissionProvider;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UserTextTranslator>().For<IUserTextTranslator>();
			system.UseTestDouble<FakeUserUiCulture>().For<IUserUiCulture>();
			system.AddService<GroupPageViewModelFactory>();
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			
		}

		[Test]
		public void ShouldReturnAvailableGroupPagesBasedOnPeriod()
		{
			var mainPage = new ReadOnlyGroupPage()
			{
				PageName = "Main",
				PageId = Group.PageMainId
			};
			var groupPage = new ReadOnlyGroupPage
			{
				PageName = "Skill",
				PageId = Guid.NewGuid()
			};
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "site1/team1",
					SiteId = siteId,
					TeamId =  teamId,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				}
			};
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage },
				groupDetails);
			
			dynamic result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = ((IEnumerable<dynamic>)result.BusinessHierarchy).ToList();
			dynamic gp = ((IEnumerable<dynamic>)result.GroupPages).ToList()[0];
			var gpChildren = ((IEnumerable<dynamic>)orgs[0].Children).ToList();
			((int)orgs.Count).Should().Be.EqualTo(1);
			((object)orgs[0].Name).Should().Be.EqualTo("site1");
			((object)orgs[0].Id).Should().Be.EqualTo(siteId);
			((object)gp.Name).Should().Be.EqualTo(groupPage.PageName);
			gpChildren.Count().Should().Be.EqualTo(1);
			((Guid)gpChildren.Single().Id).Should().Be.EqualTo(teamId);
		}
		[Test]
		public void ShouldReturnSortedGroupPages()
		{
			var mainPage = new ReadOnlyGroupPage()
			{
				PageName = "Main",
				PageId = Group.PageMainId
			};
			var groupPage = new ReadOnlyGroupPage
			{
				PageName = "Skill",
				PageId = Guid.NewGuid()
			};
			var groupPage2 = new ReadOnlyGroupPage
			{
				PageName = "Contract",
				PageId = Guid.NewGuid()
			};

			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "Site1/Team1",
					SiteId = siteId,
					TeamId =  teamId,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "Site1/ATeam",
					SiteId = siteId,
					TeamId = Guid.NewGuid(),
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "ASite/ATeam",
					SiteId = Guid.NewGuid(),
					TeamId = Guid.NewGuid(),
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "ASkill",
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage2.PageId,
					GroupName = "aContract",
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				}
			};
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage, groupPage2 },
				groupDetails);

			dynamic result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = ((IEnumerable<dynamic>)result.BusinessHierarchy).ToList();
			var gps = ((IEnumerable<dynamic>) result.GroupPages).ToList();
			dynamic gp0 = gps[0];
			dynamic gp1 = gps[1];
			var childGroupsForSite1 = ((IEnumerable<dynamic>)orgs[1].Children).ToList();
			var childGroupsForGroupPage1 = ((IEnumerable<dynamic>) gp1.Children).ToList();
			((object)orgs[0].Name).Should().Be.EqualTo("ASite");
			((object)orgs[1].Name).Should().Be.EqualTo("Site1");
			((object)childGroupsForSite1[0].Name).Should().Be.EqualTo("ATeam");
			((object)childGroupsForSite1[1].Name).Should().Be.EqualTo("Team1");
			((string)gp0.Name).Should().Be.EqualTo("Contract");
			((string)gp1.Name).Should().Be.EqualTo("Skill");
			((object)childGroupsForGroupPage1[0].Name).Should().Be.EqualTo("ASkill");
			((object)childGroupsForGroupPage1[1].Name).Should().Be.EqualTo("Email");
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldReturnLocalizedBuiltinGroupPageName()
		{
			var groupPage = new ReadOnlyGroupPage
			{
				PageName = "xxSkill",
				PageId = Guid.NewGuid()
			};
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					SiteId = siteId,
					TeamId =  teamId,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				}

			};

			GroupingReadOnlyRepository.Has(new[] { groupPage },
				groupDetails);

			dynamic result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var gps = ((IEnumerable<dynamic>)result.GroupPages).ToList();
			dynamic gp0 = gps[0];
			((string)gp0.Name).Should().Be.EqualTo("Kompetens");
		}

		[Test]
		public void ShouldOrderGroupPagesByUserCulture()
		{
			UserCulture.IsSwedish();
			var mainPage = new ReadOnlyGroupPage
			{
				PageName = "Main",
				PageId = Group.PageMainId
			};

			var groupPage = new ReadOnlyGroupPage
			{
				PageName = "a",
				PageId = Guid.NewGuid()
			};
			var groupPage1 = new ReadOnlyGroupPage
			{
				PageName = "b",
				PageId = Guid.NewGuid()
			};
			var groupPage2 = new ReadOnlyGroupPage
			{
				PageName = "åå",
				PageId = Guid.NewGuid()
			};
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "cEmail",
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "åEmail",
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "Site1/Team1",
					SiteId = siteId,
					TeamId = Guid.NewGuid(),
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "Site1/åTeam",
					SiteId = siteId,
					TeamId = Guid.NewGuid(),
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "åSite/ATeam",
					SiteId = Guid.NewGuid(),
					TeamId = Guid.NewGuid(),
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				}
			};

			GroupingReadOnlyRepository.Has(new[] {mainPage, groupPage,groupPage1,groupPage2 },
				groupDetails);

			dynamic result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var ogs = ((IEnumerable<dynamic>)result.BusinessHierarchy).ToList();
			var childrenForFirstSite = ((IEnumerable<dynamic>)ogs[0].Children).ToList();
			((string)ogs[0].Name).Should().Be.EqualTo("Site1");
			((string)ogs[1].Name).Should().Be.EqualTo("åSite");
			((string)childrenForFirstSite[0].Name).Should().Be.EqualTo("Team1");
			((string)childrenForFirstSite[1].Name).Should().Be.EqualTo("åTeam");

			var gps = ((IEnumerable<dynamic>)result.GroupPages).ToList();
			((string)gps[0].Name).Should().Be.EqualTo("a");
			((string)gps[1].Name).Should().Be.EqualTo("b");
			((string)gps[2].Name).Should().Be.EqualTo("åå");
			var childrenForFirstGroupPage = ((IEnumerable<dynamic>)gps[0].Children).ToList();
			((string)childrenForFirstGroupPage[0].Name).Should().Be.EqualTo("cEmail");
			((string)childrenForFirstGroupPage[1].Name).Should().Be.EqualTo("åEmail");
		}

		[Test]
		public void ShouldReturnPermittedGroups()
		{
			var mainPage = new ReadOnlyGroupPage()
			{
				PageName = "Main",
				PageId = Group.PageMainId
			};
			var groupPage = new ReadOnlyGroupPage
			{
				PageName = "Skill",
				PageId = Guid.NewGuid()
			};
			var businessUnitId = Guid.NewGuid();
			var site1Id = Guid.NewGuid();
			var site2Id = Guid.NewGuid();
			var team1InSite1Id = Guid.NewGuid();
			var team1InSite2Id = Guid.NewGuid();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "site1/team1",
					SiteId = site1Id,
					TeamId =  team1InSite1Id,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = "site2/team1",
					SiteId = site2Id,
					TeamId =  team1InSite2Id,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					SiteId = site1Id,
					TeamId = team1InSite1Id,
					GroupId = team1InSite1Id,
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Outbound",
					SiteId = site2Id,
					TeamId = team1InSite2Id,
					GroupId = team1InSite2Id,
					BusinessUnitId = businessUnitId
				}
			};
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage },
				groupDetails);
			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, DateOnly.Today, new PersonAuthorization
			{
				SiteId = site1Id,
				TeamId = team1InSite1Id,
				BusinessUnitId = businessUnitId
			});

			dynamic result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = ((IEnumerable<dynamic>)result.BusinessHierarchy).ToList();
			var gps = ((IEnumerable<dynamic>) result.GroupPages).ToList();
			dynamic gp = gps[0];
			var gpChildren = ((IEnumerable<dynamic>)orgs[0].Children).ToList();
			orgs.Count.Should().Be.EqualTo(1);
			((object)orgs[0].Name).Should().Be.EqualTo("site1");
			((object)orgs[0].Id).Should().Be.EqualTo(site1Id);
			((IEnumerable<dynamic>)orgs[0].Children).Count().Should().Be.EqualTo(1);
			gps.Count.Should().Be.EqualTo(1);
			((object)gp.Name).Should().Be.EqualTo(groupPage.PageName);
			gpChildren.Count.Should().Be.EqualTo(1);
			((Guid)gpChildren.Single().Id).Should().Be.EqualTo(team1InSite1Id);
		}

	}

}
