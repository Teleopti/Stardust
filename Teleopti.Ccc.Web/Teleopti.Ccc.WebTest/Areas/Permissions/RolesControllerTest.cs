using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	[PermissionsTest]
	[NoDefaultData]
	public class RolesControllerTest
	{
		public RolesController Target;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public IApplicationFunctionRepository ApplicationFunctionRepository;
		public ISiteRepository SiteRepository;
		public ITeamRepository TeamRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IPersonRepository PersonRepository;
		public FakeLoggedOnUser LoggedOnUser;
		
		[Test]
		public void ShouldCreateNewRole()
		{
			Target.Request = new HttpRequestMessage();
			Target.Post(new NewRoleInput {Description = "Site Administrator"});

			ApplicationRoleRepository.LoadAll().First().Name.Should().Be.EqualTo("SiteAdministrator");
			ApplicationRoleRepository.LoadAll().First().DescriptionText.Should().Be.EqualTo("Site Administrator");
		}

		[Test]
		public void ShouldNotCreateNewRoleWhenGivenNameTooLong()
		{
			Target.Request = new HttpRequestMessage();
			var result = Target.Post(new NewRoleInput {Description = "Admin".PadRight(256, 'a')});
			result.Should().Be.OfType<BadRequestErrorMessageResult>();
		}

		[Test]
		public void ShouldGetAllRolesAvailable()
		{
			var agentRole = new ApplicationRole {Name = "Agent"};
			ApplicationRoleRepository.Add(agentRole);
			var adminRole = new ApplicationRole {Name = "Admin", BuiltIn = true};
			ApplicationRoleRepository.Add(adminRole);
			var result = Target.Get();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSeeIfCurrentUserBelongsToRoles()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var myRole = person.PermissionInformation.ApplicationRoleCollection.First();
			ApplicationRoleRepository.Add(myRole);
			var adminRole = new ApplicationRole { Name = "SuperduperAdmin", BuiltIn = true };
			adminRole.WithId();
			ApplicationRoleRepository.Add(adminRole);

			var result = Target.Get();

			var myReturnedRole = result.First(x => x.Name == myRole.Name);
			var adminReturnedRole = result.First(x => x.Name == adminRole.Name);
			myReturnedRole.IsMyRole.Should().Be.EqualTo(true);
			adminReturnedRole.IsMyRole.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldAddNewFunctionsToRole()
		{
			var functionOneId = Guid.NewGuid();
			
			var functionOne = new ApplicationFunction("FunctionOne");
			ApplicationFunctionRepository.Add(functionOne);
			var agentRole = new ApplicationRole {Name = "Agent"};
			agentRole.WithId();
			agentRole.AddApplicationFunction(functionOne);
			ApplicationRoleRepository.Add(agentRole);
			Target.AddFunctions(agentRole.Id.Value, new FunctionsForRoleInput {Functions = new Collection<Guid> {functionOneId}});

			agentRole.ApplicationFunctionCollection.Should().Contain(functionOne);
		}

		[Test]
		public void ShouldRemoveAvailableBusinessUnitFromRole()
		{			
			var businessUnit = BusinessUnitFactory.CreateWithId("Bu 2");
			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()};
			agentRole.WithId();
			agentRole.AvailableData.AddAvailableBusinessUnit(businessUnit);
			ApplicationRoleRepository.Add(agentRole);

			Target.RemoveAvailableBusinessUnit(agentRole.Id.Value, businessUnit.Id.GetValueOrDefault());

			agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveAvailableTeamFromRole()
		{		
			var team = TeamFactory.CreateSimpleTeam();
			team.SetId(Guid.NewGuid());
			team.Site = SiteFactory.CreateSimpleSite().WithId();
			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()};
			agentRole.WithId();
			agentRole.AvailableData.AddAvailableTeam(team);
			ApplicationRoleRepository.Add(agentRole);
			Target.RemoveAvailableTeam(agentRole.Id.Value, team.Id.GetValueOrDefault());

			agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveSiteThatTeamBelongsToWhenRemovingAvailableTeamAndMakingWholeSiteNotAvailableFromRole()
		{		
			var team1 = TeamFactory.CreateSimpleTeam().WithId();
			var team2 = TeamFactory.CreateSimpleTeam().WithId();
			var site = SiteFactory.CreateSimpleSite().WithId();
			team1.Site = site;
			team2.Site = site;

			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()}.WithId();

			agentRole.AvailableData.AddAvailableTeam(team1);
			agentRole.AvailableData.AddAvailableTeam(team2);
			agentRole.AvailableData.AddAvailableSite(site);
			ApplicationRoleRepository.Add(agentRole);
			Target.RemoveAvailableTeam(agentRole.Id.Value, team1.Id.GetValueOrDefault());

			agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(1);
			agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveAvailableSiteFromRole()
		{			
			var site = SiteFactory.CreateSimpleSite();
			site.SetId(Guid.NewGuid());
			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()};
			agentRole.WithId();
			agentRole.AvailableData.AddAvailableSite(site);
			ApplicationRoleRepository.Add(agentRole);

			Target.RemoveAvailableSite(agentRole.Id.Value, site.Id.GetValueOrDefault());

			agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNotAddNewAvailableDataToBuiltInRole()
		{			
			var businessUnitId = Guid.NewGuid();
			var agentRole = new ApplicationRole {Name = "Agent", BuiltIn = true, AvailableData = new AvailableData()};
			agentRole.WithId();
			ApplicationRoleRepository.Add(agentRole);
			Target.AddAvailableData(agentRole.Id.Value,
				new AvailableDataForRoleInput
				{
					BusinessUnits = new Collection<Guid> {businessUnitId}
				});

			agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldRenameRole()
		{			
			var agentRole = new ApplicationRole {Name = "Agent", DescriptionText = "Agent"};
			agentRole.WithId();
			ApplicationRoleRepository.Add(agentRole);
			var result = Target.RenameRole(agentRole.Id.Value, new RoleNameInput {NewDescription = "Self service agent"});

			agentRole.DescriptionText.Should().Be.EqualTo("Self service agent");
			agentRole.Name.Should().Be.EqualTo("Selfserviceagent");
			result.Should().Be.OfType<OkResult>();
		}

		[Test]
		public void ShouldNotRenameBuiltInRole()
		{		
			var agentRole = new ApplicationRole {Name = "Agent", BuiltIn = true};
			agentRole.WithId();
			ApplicationRoleRepository.Add(agentRole);
			var response = Target.RenameRole(agentRole.Id.Value, new RoleNameInput {NewDescription = "Self service agent"});

			agentRole.Name.Should().Be.EqualTo("Agent");
			response.Should().Be.OfType<BadRequestErrorMessageResult>();
		}

		[Test]
		public void ShouldNotAllowRenameToEmptyName()
		{
			var roleId = Guid.NewGuid();
			var agentRole = new ApplicationRole {Name = "Agent", BuiltIn = true};
			ApplicationRoleRepository.Add(agentRole);
			var response = Target.RenameRole(roleId, new RoleNameInput {NewDescription = ""});

			agentRole.Name.Should().Be.EqualTo("Agent");
			response.Should().Be.OfType<BadRequestErrorMessageResult>();
		}

		[Test]
		public void ShouldNotAddNewFunctionsToBuiltInRole()
		{
			var functionOneId = Guid.NewGuid();			
			var functionOne = new ApplicationFunction("FunctionOne");
			ApplicationFunctionRepository.Add(functionOne);
			var agentRole = new ApplicationRole {Name = "Agent", BuiltIn = true};
			agentRole.WithId();
			ApplicationRoleRepository.Add(agentRole);
			Target.AddFunctions(agentRole.Id.Value, new FunctionsForRoleInput {Functions = new Collection<Guid> {functionOneId}});

			agentRole.ApplicationFunctionCollection.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAddNewFunctionsToMyRole()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var role = person.PermissionInformation.ApplicationRoleCollection.First();

			while (role.ApplicationFunctionCollection.Any())
				role.RemoveApplicationFunction(role.ApplicationFunctionCollection.First());

			var functionOneId = Guid.NewGuid();
			var functionOne = new ApplicationFunction("FunctionOne");
			ApplicationFunctionRepository.Add(functionOne);
			ApplicationRoleRepository.Add(role);
			Target.AddFunctions(role.Id.Value, new FunctionsForRoleInput {Functions = new Collection<Guid> {functionOneId}});

			role.ApplicationFunctionCollection.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddNewFunctionsToMyRoleIfOtherRoleIsBuiltIn()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var role = person.PermissionInformation.ApplicationRoleCollection.First();

			while (role.ApplicationFunctionCollection.Any())
				role.RemoveApplicationFunction(role.ApplicationFunctionCollection.First());

			var functionOneId = Guid.NewGuid();
			var functionOne = new ApplicationFunction("FunctionOne");
			ApplicationFunctionRepository.Add(functionOne);
			ApplicationRoleRepository.Add(role);

			var roleOneBuiltInId = Guid.NewGuid();
			var agentBuiltInRole = new ApplicationRole { Name = "Super Agent", BuiltIn = true };

			agentBuiltInRole.SetId(roleOneBuiltInId);
			ApplicationRoleRepository.Add(agentBuiltInRole);
			person.PermissionInformation.AddApplicationRole(agentBuiltInRole);

			Target.AddFunctions(role.Id.Value, new FunctionsForRoleInput {Functions = new Collection<Guid> {functionOneId}});

			role.ApplicationFunctionCollection.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldRemoveFunctionsFromRole()
		{
			var functionOneId = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var functionOne = new ApplicationFunction("FunctionOne");
			functionOne.SetId(functionOneId);
			ApplicationFunctionRepository.Add(functionOne);
			var agentRole = new ApplicationRole {Name = "Agent"};
			agentRole.SetId(roleId);
			agentRole.AddApplicationFunction(functionOne);
			ApplicationRoleRepository.Add(agentRole);
			Target.RemoveFunction(roleId, functionOneId);

			agentRole.ApplicationFunctionCollection.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotRemoveFunctionsFromBuiltInRole()
		{
			var functionOneId = Guid.NewGuid();
			var roleId = Guid.NewGuid();
			var functionOne = new ApplicationFunction("FunctionOne");
			functionOne.SetId(functionOneId);
			ApplicationFunctionRepository.Add(functionOne);
			var agentRole = new ApplicationRole {Name = "Agent", BuiltIn = true};
			agentRole.SetId(roleId);
			agentRole.AddApplicationFunction(functionOne);
			ApplicationRoleRepository.Add(agentRole);
			agentRole.AddApplicationFunction(functionOne);
			Target.RemoveFunction(roleId, functionOneId);

			agentRole.ApplicationFunctionCollection.Should().Contain(functionOne);
		}

		[Test]
		public void ShouldNotRemoveFunctionsFromMyRole()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var role = person.PermissionInformation.ApplicationRoleCollection.First();

			var functionOne = role.ApplicationFunctionCollection.First();
			ApplicationFunctionRepository.Add(functionOne);
			ApplicationRoleRepository.Add(role);
			Target.RemoveFunction(role.Id.Value, functionOne.Id.Value);

			role.ApplicationFunctionCollection.Should().Contain(functionOne);
		}
		
		[Test]
		public void ShouldRemoveFunctionsFromMyRoleIfOtherRoleIsBuiltIn()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var role = person.PermissionInformation.ApplicationRoleCollection.First();

			var functionOne = role.ApplicationFunctionCollection.First();
			ApplicationFunctionRepository.Add(functionOne);
			ApplicationRoleRepository.Add(role);

			var functionBuiltInId = Guid.NewGuid();
			var roleOneBuiltInId = Guid.NewGuid();
			var functionBuiltIn = new ApplicationFunction("functionBuiltIn");
			functionBuiltIn.SetId(functionBuiltInId);
			ApplicationFunctionRepository.Add(functionBuiltIn);
			var agentBuiltInRole = new ApplicationRole { Name = "Super Agent", BuiltIn = true };
			agentBuiltInRole.SetId(roleOneBuiltInId);
			agentBuiltInRole.AddApplicationFunction(functionOne);
			person.PermissionInformation.AddApplicationRole(agentBuiltInRole);

			Target.RemoveFunction(role.Id.Value, functionOne.Id.Value);

			role.ApplicationFunctionCollection.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldCopyExistingRole()
		{
			Target.Request = new HttpRequestMessage();
			var team = TeamFactory.CreateTeam("Team 1", "Paris");
			
			var functionOne = new ApplicationFunction("FunctionOne");
			var agentRole = new ApplicationRole
			{
				Name = "Agent",
				DescriptionText = "Agent",
				BuiltIn = true,
				AvailableData = new AvailableData()
			};
			agentRole.WithId();
			agentRole.AddApplicationFunction(functionOne);
			agentRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MySite;
			agentRole.AvailableData.AddAvailableBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			agentRole.AvailableData.AddAvailableSite(team.Site);
			agentRole.AvailableData.AddAvailableTeam(team);
			ApplicationRoleRepository.Add(agentRole);

			Target.CopyExistingRole(agentRole.Id.Value);

			ApplicationRoleRepository.LoadAll()
				.Last()
				.DescriptionText.Should()
				.Be.EqualTo("Copy of " + agentRole.DescriptionText);
			ApplicationRoleRepository.LoadAll().Last().Name.Should().Be.EqualTo("Copyof" + agentRole.Name);
			ApplicationRoleRepository.LoadAll().Last().ApplicationFunctionCollection.Should().Have.Count.EqualTo(1);
			ApplicationRoleRepository.LoadAll().Last().AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(1);
			ApplicationRoleRepository.LoadAll().Last().AvailableData.AvailableSites.Should().Have.Count.EqualTo(1);
			ApplicationRoleRepository.LoadAll().Last().AvailableData.AvailableTeams.Should().Have.Count.EqualTo(1);
			ApplicationRoleRepository.LoadAll()
				.Last()
				.AvailableData.AvailableDataRange.Should()
				.Be.EqualTo(AvailableDataRangeOption.MySite);
		}

		[Test]
		public void ShouldDeleteRole()
		{
			var roleId = Guid.NewGuid();
			var role = new ApplicationRole();
			role.SetId(roleId);
			ApplicationRoleRepository.Add(role);
			Target.Delete(roleId);
			ApplicationRoleRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotDeleteBuiltInRole()
		{
			var roleId = Guid.NewGuid();
			var role = new ApplicationRole { BuiltIn = true };
			role.SetId(roleId);
			ApplicationRoleRepository.Add(role);
			Target.Delete(roleId);
			ApplicationRoleRepository.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotDeleteMyRole()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var role = person.PermissionInformation.ApplicationRoleCollection.First();
			ApplicationRoleRepository.Add(role);
			Target.Delete(role.Id.Value);
			ApplicationRoleRepository.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldDeleteMyRoleIfOtherRoleIsBuiltIn()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var role = person.PermissionInformation.ApplicationRoleCollection.First();
			ApplicationRoleRepository.Add(role);
			
			var roleOneBuiltInId = Guid.NewGuid();
			var agentBuiltInRole = new ApplicationRole { Name = "Super Agent", BuiltIn = true };

			agentBuiltInRole.SetId(roleOneBuiltInId);
			ApplicationRoleRepository.Add(agentBuiltInRole);
			person.PermissionInformation.AddApplicationRole(agentBuiltInRole);
			Target.Delete(role.Id.Value);
			ApplicationRoleRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetFunctionsAndAvailableDataForRole()
		{
			var agentRole = new ApplicationRole {Name = "Agent"};
			agentRole.SetId(Guid.NewGuid());
			agentRole.AddApplicationFunction(new ApplicationFunction(DefinedRaptorApplicationFunctionPaths.WebPermissions));
			agentRole.AvailableData = new AvailableData();
			agentRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyOwn;
			var simpleTeam = TeamFactory.CreateTeam("Team 1", "Paris");
			agentRole.AvailableData.AddAvailableTeam(simpleTeam);
			agentRole.AvailableData.AddAvailableSite(simpleTeam.Site);
			agentRole.AvailableData.AddAvailableBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			ApplicationRoleRepository.Add(agentRole);
			dynamic result = Target.Get(agentRole.Id.GetValueOrDefault());

			((string) result.Name).Should().Be.EqualTo(agentRole.Name);
			((AvailableDataRangeOption) result.AvailableDataRange).Should().Be.EqualTo(AvailableDataRangeOption.MyOwn);
			((ICollection<object>) result.AvailableTeams).Count.Should().Be.EqualTo(1);
			((ICollection<object>) result.AvailableSites).Count.Should().Be.EqualTo(1);
			((ICollection<object>) result.AvailableBusinessUnits).Count.Should().Be.EqualTo(1);
			((ICollection<object>) result.AvailableFunctions).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveFunctionWitChild()
		{
			
			var parentId = Guid.NewGuid();
			var childId = Guid.NewGuid();
			var grandChildId = Guid.NewGuid();
			var parent = new ApplicationFunction("parent");
			parent.SetId(parentId);
			ApplicationFunctionRepository.Add(parent);
			var child = new ApplicationFunction("child");
			child.SetId(childId);
			ApplicationFunctionRepository.Add(child);
			var grandChild = new ApplicationFunction("grandchild");
			grandChild.SetId(grandChildId);
			ApplicationFunctionRepository.Add(grandChild);
			child.AddChild(grandChild);
			parent.AddChild(child);
			var agentRole = new ApplicationRole {Name = "Agent"};
			agentRole.WithId();
			ApplicationRoleRepository.Add(agentRole);
			agentRole.AddApplicationFunction(parent);
			agentRole.AddApplicationFunction(child);
			agentRole.AddApplicationFunction(grandChild);
			Target.RemoveFunction(agentRole.Id.Value, parentId);

			agentRole.ApplicationFunctionCollection.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveAvailableSiteWithTeamsFromRole()
		{			
			var team = TeamFactory.CreateSimpleTeam();
			var site = SiteFactory.CreateSimpleSite();
			var siteId = Guid.NewGuid();

			site.SetId(siteId);
			team.SetId(Guid.NewGuid());
			team.Site = site;
			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()};
			agentRole.WithId();
			agentRole.AvailableData.AddAvailableTeam(team);

			agentRole.AvailableData.AddAvailableSite(site);
			ApplicationRoleRepository.Add(agentRole);

			Target.RemoveAvailableSite(agentRole.Id.Value, site.Id.GetValueOrDefault());

			agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
			agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveAvailableBusinessUnitSiteAndTeamFromRole()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Bu 2");
			System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
				new TeleoptiIdentity("test", null, () => businessUnit.Id, businessUnit.Name, null, null), new PersonAndBusinessUnit(PersonFactory.CreatePerson(), null));			
			var site = SiteFactory.CreateSimpleSite();

			businessUnit.SetId(site.BusinessUnit.Id);
			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()};
			agentRole.WithId();
			agentRole.AvailableData.AddAvailableBusinessUnit(businessUnit);
			ApplicationRoleRepository.Add(agentRole);


			var team = TeamFactory.CreateSimpleTeam();

			var siteId = Guid.NewGuid();

			site.SetId(siteId);
			team.SetId(Guid.NewGuid());
			team.Site = site;

			agentRole.AvailableData.AddAvailableTeam(team);

			agentRole.AvailableData.AddAvailableSite(site);
			ApplicationRoleRepository.Add(agentRole);

			Target.RemoveAvailableBusinessUnit(agentRole.Id.Value, businessUnit.Id.GetValueOrDefault());

			agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(0);
			agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
			agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveFunctionWithChildAndAllParents()
		{			
			var iamNoParentId = Guid.NewGuid();
			var parentOfEverythingId = Guid.NewGuid();
			var grandChildId = Guid.NewGuid();
			var iAmNoParent = new ApplicationFunction("All");
			iAmNoParent.SetId(iamNoParentId);
			ApplicationFunctionRepository.Add(iAmNoParent);

			var parentOfEverything = new ApplicationFunction("Teleopti");
			parentOfEverything.SetId(parentOfEverythingId);
			ApplicationFunctionRepository.Add(parentOfEverything);

			var grandChild = new ApplicationFunction("Prog");
			grandChild.SetId(grandChildId);
			ApplicationFunctionRepository.Add(grandChild);
			parentOfEverything.AddChild(grandChild);

			var agentRole = new ApplicationRole {Name = "Agent"};
			agentRole.WithId();
			ApplicationRoleRepository.Add(agentRole);
			agentRole.AddApplicationFunction(iAmNoParent);
			agentRole.AddApplicationFunction(parentOfEverything);
			agentRole.AddApplicationFunction(grandChild);

			Target.RemoveFunction(agentRole.Id.Value, grandChildId,
				new FunctionsForRoleInput() {Functions = new Guid[] {iamNoParentId, parentOfEverythingId}});

			agentRole.ApplicationFunctionCollection.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotRemoveAllFunctionsIfMyRole()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var myRole = person.PermissionInformation.ApplicationRoleCollection.First();
			ApplicationRoleRepository.Add(myRole);
			var theFunction = person.PermissionInformation.ApplicationRoleCollection.First().ApplicationFunctionCollection.First();
			ApplicationFunctionRepository.Add(theFunction);
			
			Target.RemoveFunction(myRole.Id.Value, theFunction.Id.Value,
				new FunctionsForRoleInput() { Functions = new Guid[0] });

			myRole.ApplicationFunctionCollection.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveAllFunctionsOfMyRoleIfOtherRoleIsBuiltIn()
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var myRole = person.PermissionInformation.ApplicationRoleCollection.First();
			ApplicationRoleRepository.Add(myRole);
			var theFunction = person.PermissionInformation.ApplicationRoleCollection.First().ApplicationFunctionCollection.First();
			ApplicationFunctionRepository.Add(theFunction);

			var roleOneBuiltInId = Guid.NewGuid();
			var agentBuiltInRole = new ApplicationRole { Name = "Super Agent", BuiltIn = true };

			agentBuiltInRole.SetId(roleOneBuiltInId);
			ApplicationRoleRepository.Add(agentBuiltInRole);
			person.PermissionInformation.AddApplicationRole(agentBuiltInRole);

			Target.RemoveFunction(myRole.Id.Value, theFunction.Id.Value,
				new FunctionsForRoleInput() { Functions = new Guid[0] });

			myRole.ApplicationFunctionCollection.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldAddNewAvailableDataToRole()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Businessunit");
			BusinessUnitRepository.Add(businessUnit);

			var team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			var site = SiteFactory.CreateSimpleSite("site");
			var siteId = Guid.NewGuid();
			site.SetId(siteId);
			SiteRepository.Add(site);

			var roleId = Guid.NewGuid();
			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()};
			agentRole.SetId(roleId);
			ApplicationRoleRepository.Add(agentRole);

			Target.AddAvailableData(roleId,
				new AvailableDataForRoleInput
				{
					BusinessUnits = new Collection<Guid> {businessUnit.Id.Value},
					Sites = new Collection<Guid> {siteId},
					Teams = new Collection<Guid> {team.Id.Value},
					RangeOption = AvailableDataRangeOption.MyBusinessUnit
				});

			agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(1);
			agentRole.AvailableData.AvailableBusinessUnits.First().Name.Should().Be.EqualTo(businessUnit.Name);

			agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(1);
			agentRole.AvailableData.AvailableSites.First().Description.Should().Be.EqualTo(site.Description);

			agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(1);
			agentRole.AvailableData.AvailableTeams.First().Description.Should().Be.EqualTo(team.Description);
		}

		[Test]
		public void ShouldDeleteParentAndAllChildren()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Bu 2");
			System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
				new TeleoptiIdentity("test", null, () => businessUnit.Id, businessUnit.Name, null, null), new PersonAndBusinessUnit(PersonFactory.CreatePerson(), null));			
			var site = SiteFactory.CreateSimpleSite();

			businessUnit.SetId(site.BusinessUnit.Id);
			var agentRole = new ApplicationRole {Name = "Agent", AvailableData = new AvailableData()};
			agentRole.WithId();
			agentRole.AvailableData.AddAvailableBusinessUnit(businessUnit);
			ApplicationRoleRepository.Add(agentRole);

			var team = TeamFactory.CreateSimpleTeam();

			var siteId = Guid.NewGuid();


			site.SetId(siteId);
			team.SetId(Guid.NewGuid());
			team.Site = site;



			var data = new AvailableDataForRoleInput
			{
				BusinessUnits = new Collection<Guid> {businessUnit.Id.Value},
				Sites = new Collection<Guid> {siteId},
				Teams = new Collection<Guid> {team.Id.Value},
			};


			agentRole.AvailableData.AddAvailableTeam(team);

			agentRole.AvailableData.AddAvailableSite(site);
			ApplicationRoleRepository.Add(agentRole);

			Target.RemoveAvailable(agentRole.Id.Value, data);

			agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(0);
			agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
			agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(0);
		}

	}
}


