﻿using System;
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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global.Core
{

	[TestFixture, DomainTest]
	public class GroupPageViewModelFactoryTest : ISetup
	{
		public GroupPageViewModelFactory Target;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakeUserUiCulture UserCulture;
		public FakePermissionProvider PermissionProvider;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeOptionalColumnRepository OptionalColumnRepository;
		public FakePersonRepository PersonRepository;
		public ITeamRepository TeamRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UserTextTranslator>().For<IUserTextTranslator>();
			system.UseTestDouble<FakeUserUiCulture>().For<IUserUiCulture>();
			system.AddService<GroupPageViewModelFactory>();
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeOptionalColumnRepository>().For<IOptionalColumnRepository>();
		}


		[Test]
		public void ShouldReturnAvailableDynamicOptionalColumnGroupPage()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var optionalColumnWithGroup = new OptionalColumn("test").WithId();
			optionalColumnWithGroup.AvailableAsGroupPage = true;
			var optionalColumnNoGroup = new OptionalColumn("no group").WithId();
			OptionalColumnRepository.Add(optionalColumnWithGroup);
			OptionalColumnRepository.Add(optionalColumnNoGroup);

			var valueForTest = new OptionalColumnValue("my value").WithId();
			var valueForNoGroup = new OptionalColumnValue("another").WithId();
			person.SetOptionalColumnValue(valueForTest, optionalColumnWithGroup);
			person.SetOptionalColumnValue(valueForNoGroup, optionalColumnNoGroup);

			PersonRepository.Add(person);
			OptionalColumnRepository.AddPersonValues(valueForTest);
			OptionalColumnRepository.AddPersonValues(valueForNoGroup);
			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var gps = result.GroupPages;

			gps.Single().Name.Should().Be.EqualTo(optionalColumnWithGroup.Name);
			gps.Single().Children.Single().Name.Should().Be.EqualTo(valueForTest.Description);
		}


		[Test]
		public void ShouldReturnAvailableDynamicOptionalColumnGroupPageWithValues()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var optionalColumnNoValue = new OptionalColumn("test").WithId();
			optionalColumnNoValue.AvailableAsGroupPage = true;
			var optionalColumnWithValue = new OptionalColumn("another").WithId();
			optionalColumnWithValue.AvailableAsGroupPage = true;
			OptionalColumnRepository.Add(optionalColumnNoValue);
			OptionalColumnRepository.Add(optionalColumnWithValue);

			var valueForAnother = new OptionalColumnValue("another").WithId();
			person.SetOptionalColumnValue(valueForAnother, optionalColumnWithValue);
			OptionalColumnRepository.AddPersonValues(valueForAnother);

			PersonRepository.Add(person);
			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var gps = result.GroupPages;

			gps.Single().Name.Should().Be.EqualTo(optionalColumnWithValue.Name);
			gps.Single().Children.Single().Name.Should().Be.EqualTo(valueForAnother.Description);
		}

		[Test]
		public void ShouldNotReturnAvailableDynamicOptionalColumnGroupPageWithValuesReferingToDeletedPersonOnly()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var personDeleted = PersonFactory.CreatePerson().WithId();
			(personDeleted as Person).SetDeleted();
			var optionalColumnNoValue = new OptionalColumn("test").WithId();
			optionalColumnNoValue.AvailableAsGroupPage = true;
			var optionalColumnWithValue = new OptionalColumn("another").WithId();
			optionalColumnWithValue.AvailableAsGroupPage = true;
			OptionalColumnRepository.Add(optionalColumnNoValue);
			OptionalColumnRepository.Add(optionalColumnWithValue);

			var valueForAnother = new OptionalColumnValue("another").WithId();
			var valueDeletedForAnother = new OptionalColumnValue("deleted for another").WithId();
			person.SetOptionalColumnValue(valueForAnother, optionalColumnWithValue);
			personDeleted.SetOptionalColumnValue(valueDeletedForAnother, optionalColumnWithValue);
			OptionalColumnRepository.AddPersonValues(valueForAnother);
			OptionalColumnRepository.AddPersonValues(valueDeletedForAnother);

			PersonRepository.Add(person);
			PersonRepository.Add(personDeleted);
			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var gps = result.GroupPages;

			gps.Single().Name.Should().Be.EqualTo(optionalColumnWithValue.Name);
			gps.Single().Children.Single().Name.Should().Be.EqualTo(valueForAnother.Description);
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
			var team = TeamFactory.CreateTeam("team1", "site1").WithId();
			team.Site.WithId();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team.SiteAndTeam,
					SiteId = team.Site.Id,
					TeamId =  team.Id,
					GroupId = team.Id.GetValueOrDefault(),
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
			TeamRepository.Add(team);
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage },
				groupDetails);

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = result.BusinessHierarchy.ToList();
			var gp = result.GroupPages[0];
			var gpChildren = orgs[0].Children;
			orgs.Count.Should().Be.EqualTo(1);
			orgs[0].Name.Should().Be.EqualTo("site1");
			orgs[0].Id.Should().Be.EqualTo(team.Site.Id);
			gp.Name.Should().Be.EqualTo(groupPage.PageName);
			gpChildren.Count.Should().Be.EqualTo(1);
			gpChildren.Single().Id.Should().Be.EqualTo(team.Id);
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
			var team1 = TeamFactory.CreateTeam("Team1", "Site1").WithId();
			team1.Site.WithId(siteId);

			var team2 = TeamFactory.CreateTeam("ATeam", "Site1").WithId();
			team2.Site.WithId(siteId);

			var team3 = TeamFactory.CreateTeam("ATeam", "ASite").WithId();
			team3.Site.WithId();

			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team1.SiteAndTeam,
					SiteId = siteId,
					TeamId =  team1.Id,
					GroupId = team1.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team2.SiteAndTeam,
					SiteId = siteId,
					TeamId = team2.Id,
					GroupId = team2.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team3.SiteAndTeam,
					SiteId = team3.Site.Id,
					TeamId = team3.Id,
					GroupId = team3.Id.GetValueOrDefault(),
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
			TeamRepository.Add(team1);
			TeamRepository.Add(team2);
			TeamRepository.Add(team3);
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage, groupPage2 },
				groupDetails);

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = result.BusinessHierarchy;
			var gps = result.GroupPages;
			var gp0 = gps[0];
			var gp1 = gps[1];
			var childGroupsForSite1 = orgs[1].Children;
			var childGroupsForGroupPage1 = gp1.Children;
			orgs[0].Name.Should().Be.EqualTo("ASite");
			orgs[1].Name.Should().Be.EqualTo("Site1");
			childGroupsForSite1[0].Name.Should().Be.EqualTo("ATeam");
			childGroupsForSite1[1].Name.Should().Be.EqualTo("Team1");
			gp0.Name.Should().Be.EqualTo("Contract");
			gp1.Name.Should().Be.EqualTo("Skill");
			childGroupsForGroupPage1[0].Name.Should().Be.EqualTo("ASkill");
			childGroupsForGroupPage1[1].Name.Should().Be.EqualTo("Email");
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

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var gps = result.GroupPages;
			var gp0 = gps[0];
			gp0.Name.Should().Be.EqualTo("Kompetens");
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
			var team1 = TeamFactory.CreateTeam("Team1", "Site1").WithId();
			team1.Site.WithId(siteId);

			var team2 = TeamFactory.CreateTeam("åTeam", "Site1").WithId();
			team2.Site.WithId(siteId);

			var team3 = TeamFactory.CreateTeam("ATeam", "åSite").WithId();
			team3.Site.WithId();

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
					GroupName = team1.SiteAndTeam,
					SiteId = team1.Site.Id,
					TeamId = team1.Id,
					GroupId = team1.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team2.SiteAndTeam,
					SiteId = team2.Site.Id,
					TeamId = team2.Id,
					GroupId = team2.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team3.SiteAndTeam,
					SiteId = team3.Site.Id,
					TeamId = team3.Id,
					GroupId = team3.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				}
			};
			TeamRepository.Add(team1);

			TeamRepository.Add(team2);

			TeamRepository.Add(team3);

			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage, groupPage1, groupPage2 },
				groupDetails);

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var ogs = result.BusinessHierarchy;
			var childrenForFirstSite = ogs[0].Children;
			ogs[0].Name.Should().Be.EqualTo("Site1");
			ogs[1].Name.Should().Be.EqualTo("åSite");
			childrenForFirstSite[0].Name.Should().Be.EqualTo("Team1");
			childrenForFirstSite[1].Name.Should().Be.EqualTo("åTeam");

			var gps = result.GroupPages.ToList();
			gps[0].Name.Should().Be.EqualTo("a");
			gps[1].Name.Should().Be.EqualTo("b");
			gps[2].Name.Should().Be.EqualTo("åå");
			var childrenForFirstGroupPage = gps[0].Children;
			childrenForFirstGroupPage[0].Name.Should().Be.EqualTo("cEmail");
			childrenForFirstGroupPage[1].Name.Should().Be.EqualTo("åEmail");
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

			var team1Site1 = TeamFactory.CreateTeam("team1", "site1");
			team1Site1.SetId(team1InSite1Id);
			team1Site1.Site.WithId(site1Id);

			var team1Site2 = TeamFactory.CreateTeam("team1", "site2");
			team1Site2.SetId(team1InSite2Id);
			team1Site2.Site.WithId(site2Id);

			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team1Site1.SiteAndTeam,
					SiteId = site1Id,
					TeamId =  team1InSite1Id,
					GroupId = team1InSite1Id,
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team1Site2.SiteAndTeam,
					SiteId = site2Id,
					TeamId =  team1InSite2Id,
					GroupId = team1InSite2Id,
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					SiteId = site1Id,
					TeamId = team1InSite1Id,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Outbound",
					SiteId = site2Id,
					TeamId = team1InSite2Id,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				}
			};
			TeamRepository.Add(team1Site1);
			TeamRepository.Add(team1Site2);

			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage },
				groupDetails);

			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, DateOnly.Today, new PersonAuthorization
			{
				SiteId = site1Id,
				TeamId = team1InSite1Id,
				BusinessUnitId = businessUnitId
			});

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = result.BusinessHierarchy;
			var gps = result.GroupPages;
			var gp = gps[0];
			var gpChildren = orgs[0].Children;
			orgs.Length.Should().Be.EqualTo(1);
			orgs[0].Name.Should().Be.EqualTo("site1");
			orgs[0].Id.Should().Be.EqualTo(site1Id);
			orgs[0].Children.Count.Should().Be.EqualTo(1);
			gps.Length.Should().Be.EqualTo(1);
			gp.Name.Should().Be.EqualTo(groupPage.PageName);
			gpChildren.Count.Should().Be.EqualTo(1);
			gpChildren.Single().Id.Should().Be.EqualTo(team1InSite1Id);
		}

		[Test]
		public void ShouldOnlyReturnSitesWithChoosableTeams()
		{
			var mainPage = new ReadOnlyGroupPage()
			{
				PageName = "Main",
				PageId = Group.PageMainId
			};

			var businessUnitId = Guid.NewGuid();
			var site1Id = Guid.NewGuid();
			var team1InSite1Id = Guid.NewGuid();

			var team1Site1 = TeamFactory.CreateTeam("team1", "site1");
			team1Site1.SetId(team1InSite1Id);
			team1Site1.Site.WithId(site1Id);
			team1Site1.SetDeleted();

			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team1Site1.SiteAndTeam,
					SiteId = site1Id,
					TeamId =  team1InSite1Id,
					GroupId = team1InSite1Id,
					BusinessUnitId = businessUnitId
				}
			};
			TeamRepository.Add(team1Site1);

			GroupingReadOnlyRepository.Has(new[] { mainPage }, groupDetails);

			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, DateOnly.Today, new PersonAuthorization
			{
				SiteId = site1Id,
				TeamId = team1Site1.Id.Value,
				BusinessUnitId = businessUnitId
			});

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
			result.BusinessHierarchy.Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnDistincteGroupPages()
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
			var team1 = TeamFactory.CreateTeam("Team1", "Site1").WithId();
			team1.Site.WithId(siteId);

			var team2 = TeamFactory.CreateTeam("ATeam", "Site1").WithId();
			team2.Site.WithId(siteId);

			var team3 = TeamFactory.CreateTeam("ATeam", "ASite").WithId();
			team3.Site.WithId();

			var childGroupId = Guid.NewGuid();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team1.SiteAndTeam,
					SiteId = siteId,
					TeamId =  team1.Id,
					GroupId = team1.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team2.SiteAndTeam,
					SiteId = siteId,
					TeamId = team2.Id,
					GroupId = team2.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team3.SiteAndTeam,
					SiteId = team3.Site.Id,
					TeamId = team3.Id,
					GroupId = team3.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					GroupId = childGroupId,
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					GroupId = childGroupId,
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Phone",
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				}
			};
			TeamRepository.Add(team1);
			TeamRepository.Add(team2);
			TeamRepository.Add(team3);
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage },
				groupDetails);

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var gps = result.GroupPages;
			var gp0 = gps[0];
			gps.Length.Should().Be.EqualTo(1);
			gp0.Children.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnLogonUserTeamIdInViewModel()
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

			var site2Id = Guid.NewGuid();
			var site = SiteFactory.CreateSiteWithOneTeam("mySite").WithId();
			var site1Id = Guid.NewGuid();

			var team1InSite1Id = site.TeamCollection.Single().WithId().Id;
			var team1InSite2Id = Guid.NewGuid();

			var team1Site1 = TeamFactory.CreateTeam("team1", "site1");
			team1Site1.SetId(team1InSite1Id);
			team1Site1.Site.WithId(site1Id);

			var team1Site2 = TeamFactory.CreateTeam("team1", "site2");
			team1Site2.SetId(team1InSite2Id);
			team1Site2.Site.WithId(site2Id);

			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team1Site1.SiteAndTeam,
					SiteId = site1Id,
					TeamId =  team1Site1.Id,
					GroupId = team1Site1.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team1Site2.SiteAndTeam,
					SiteId = site2Id,
					TeamId =  team1Site2.Id,
					GroupId = team1Site2.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Email",
					SiteId = site1Id,
					TeamId = team1InSite1Id,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				},
				new ReadOnlyGroupDetail
				{
					PageId = groupPage.PageId,
					GroupName = "Outbound",
					SiteId = site2Id,
					TeamId = team1InSite2Id,
					GroupId = Guid.NewGuid(),
					BusinessUnitId = businessUnitId
				}
			};
			TeamRepository.Add(team1Site1);
			TeamRepository.Add(team1Site2);
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage },
				groupDetails);
			var me = PersonFactory.CreatePerson("me").WithId();
			me.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, site.TeamCollection.Single()));

			LoggedOnUser.SetFakeLoggedOnUser(me);
			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, DateOnly.Today, new PersonAuthorization
			{
				SiteId = site1Id,
				TeamId = team1InSite1Id,
				BusinessUnitId = businessUnitId
			});

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = result.BusinessHierarchy;
			var gps = result.GroupPages;
			var gp = gps[0];
			var gpChildren = orgs[0].Children;
			orgs.Length.Should().Be.EqualTo(1);
			orgs[0].Name.Should().Be.EqualTo("site1");
			orgs[0].Id.Should().Be.EqualTo(site1Id);
			orgs[0].Children.Count.Should().Be.EqualTo(1);
			gps.Length.Should().Be.EqualTo(1);
			gp.Name.Should().Be.EqualTo(groupPage.PageName);
			gpChildren.Count.Should().Be.EqualTo(1);
			gpChildren.Single().Id.Should().Be.EqualTo(team1InSite1Id);
			result.LogonUserTeamId.Should().Be.EqualTo(team1InSite1Id);
		}

		[Test]
		public void ShouldReturnAvailableGroupPagesWithCorrectPageName()
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
			var team = TeamFactory.CreateTeam("team/200", "Site/1000").WithId();
			team.Site.WithId();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team.SiteAndTeam,
					SiteId = team.Site.Id,
					TeamId =  team.Id,
					GroupId = team.Id.GetValueOrDefault(),
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
			TeamRepository.Add(team);
			GroupingReadOnlyRepository.Has(new[] { mainPage, groupPage },
				groupDetails);

			var result = Target.CreateViewModel(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			var orgs = result.BusinessHierarchy.ToList();
			var gp = result.GroupPages[0];
			var gpChildren = orgs[0].Children;
			orgs.Count.Should().Be.EqualTo(1);
			orgs[0].Name.Should().Be.EqualTo("Site/1000");
			orgs[0].Id.Should().Be.EqualTo(team.Site.Id);
			orgs[0].Children[0].Name.Should().Be.EqualTo("team/200");
			gp.Name.Should().Be.EqualTo(groupPage.PageName);
			gpChildren.Count.Should().Be.EqualTo(1);
			gpChildren.Single().Id.Should().Be.EqualTo(team.Id);
		}


	}

}
