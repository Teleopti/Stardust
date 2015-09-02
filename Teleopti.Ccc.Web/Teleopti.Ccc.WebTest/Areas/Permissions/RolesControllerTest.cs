using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
    [PermissionsTest]
    public class RolesControllerTest
    {
        public RolesController Target;
        public IApplicationRoleRepository ApplicationRoleRepository;
        public IApplicationFunctionRepository ApplicationFunctionRepository;

        [Test]
        public void ShouldCreateNewRole()
        {
            Target.Request = new HttpRequestMessage();
            Target.Post(new NewRoleInput { Description = "Site Administrator" });

            ApplicationRoleRepository.LoadAll().First().Name.Should().Be.EqualTo("SiteAdministrator");
            ApplicationRoleRepository.LoadAll().First().DescriptionText.Should().Be.EqualTo("Site Administrator");
        }

        [Test]
        public void ShouldNotCreateNewRoleWhenGivenNameTooLong()
        {
            Target.Request = new HttpRequestMessage();
            var result = Target.Post(new NewRoleInput { Description = "Admin".PadRight(256, 'a') });
            result.Should().Be.OfType<BadRequestErrorMessageResult>();
        }

        [Test]
        public void ShouldGetAllRolesAvailable()
        {
            var agentRole = new ApplicationRole { Name = "Agent" };
            ApplicationRoleRepository.Add(agentRole);
            var adminRole = new ApplicationRole { Name = "Admin", BuiltIn = true };
            ApplicationRoleRepository.Add(adminRole);
            var result = Target.Get();
            result.Count.Should().Be.EqualTo(2);
        }

        [Test]
        public void ShouldAddNewFunctionsToRole()
        {
            var functionOneId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var functionOne = new ApplicationFunction("FunctionOne");
            ApplicationFunctionRepository.Add(functionOne);
            var agentRole = new ApplicationRole { Name = "Agent" };
            agentRole.AddApplicationFunction(functionOne);
            ApplicationRoleRepository.Add(agentRole);
            Target.AddFunctions(roleId, new FunctionsForRoleInput { Functions = new Collection<Guid> { functionOneId } });

            agentRole.ApplicationFunctionCollection.Should().Contain(functionOne);
        }

        [Test]
        public void ShouldAddNewAvailableDataToRole()
        {
            var roleId = Guid.NewGuid();
            var businessUnitId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var teamId = Guid.NewGuid();

            var agentRole = new ApplicationRole { Name = "Agent", AvailableData = new AvailableData() };
            ApplicationRoleRepository.Add(agentRole);
            Target.AddAvailableData(roleId,
                                                  new AvailableDataForRoleInput
                                                  {
                                                      BusinessUnits = new Collection<Guid> { businessUnitId },
                                                      Sites = new Collection<Guid> { siteId },
                                                      Teams = new Collection<Guid> { teamId },
                                                      RangeOption = AvailableDataRangeOption.MyBusinessUnit
                                                  });

            agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(1);
            agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(1);
            agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(1);
            agentRole.AvailableData.AvailableDataRange.Should().Be.EqualTo(AvailableDataRangeOption.MyBusinessUnit);
        }

        [Test]
        public void ShouldRemoveAvailableBusinessUnitFromRole()
        {
            var roleId = Guid.NewGuid();
            var businessUnit = BusinessUnitFactory.CreateWithId("Bu 2");
            var agentRole = new ApplicationRole { Name = "Agent", AvailableData = new AvailableData() };
            agentRole.AvailableData.AddAvailableBusinessUnit(businessUnit);
            ApplicationRoleRepository.Add(agentRole);

            Target.RemoveAvailableBusinessUnit(roleId, businessUnit.Id.GetValueOrDefault());

            agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(0);
        }

        [Test]
        public void ShouldRemoveAvailableTeamFromRole()
        {
            var roleId = Guid.NewGuid();
            var team = TeamFactory.CreateSimpleTeam();
            team.SetId(Guid.NewGuid());
            var agentRole = new ApplicationRole { Name = "Agent", AvailableData = new AvailableData() };
            agentRole.AvailableData.AddAvailableTeam(team);
            ApplicationRoleRepository.Add(agentRole);
            Target.RemoveAvailableTeam(roleId, team.Id.GetValueOrDefault());

            agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(0);
        }

        [Test]
        public void ShouldRemoveAvailableSiteFromRole()
        {
            var roleId = Guid.NewGuid();
            var site = SiteFactory.CreateSimpleSite();
            site.SetId(Guid.NewGuid());
            var agentRole = new ApplicationRole { Name = "Agent", AvailableData = new AvailableData() };
            agentRole.AvailableData.AddAvailableSite(site);
            ApplicationRoleRepository.Add(agentRole);

            Target.RemoveAvailableSite(roleId, site.Id.GetValueOrDefault());

            agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
        }

        [Test]
        public void ShouldNotAddNewAvailableDataToBuiltInRole()
        {
            var roleId = Guid.NewGuid();
            var businessUnitId = Guid.NewGuid();
            var agentRole = new ApplicationRole { Name = "Agent", BuiltIn = true, AvailableData = new AvailableData() };
            ApplicationRoleRepository.Add(agentRole);
            Target.AddAvailableData(roleId,
                                                  new AvailableDataForRoleInput
                                                  {
                                                      BusinessUnits = new Collection<Guid> { businessUnitId }
                                                  });

            agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(0);
        }

        [Test]
        public void ShouldRenameRole()
        {
            var roleId = Guid.NewGuid();
            var agentRole = new ApplicationRole { Name = "Agent", DescriptionText = "Agent" };
            ApplicationRoleRepository.Add(agentRole);
            var result = Target.RenameRole(roleId, new RoleNameInput { NewDescription = "Self service agent" });

            agentRole.DescriptionText.Should().Be.EqualTo("Self service agent");
            agentRole.Name.Should().Be.EqualTo("Selfserviceagent");
            result.Should().Be.OfType<OkResult>();
        }

        [Test]
        public void ShouldNotRenameBuiltInRole()
        {
            var roleId = Guid.NewGuid();
            var agentRole = new ApplicationRole { Name = "Agent", BuiltIn = true };
            ApplicationRoleRepository.Add(agentRole);
            var response = Target.RenameRole(roleId, new RoleNameInput { NewDescription = "Self service agent" });

            agentRole.Name.Should().Be.EqualTo("Agent");
            response.Should().Be.OfType<BadRequestErrorMessageResult>();
        }

        [Test]
        public void ShouldNotAllowRenameToEmptyName()
        {
            var roleId = Guid.NewGuid();
            var agentRole = new ApplicationRole { Name = "Agent", BuiltIn = true };
            ApplicationRoleRepository.Add(agentRole);
            var response = Target.RenameRole(roleId, new RoleNameInput { NewDescription = "" });

            agentRole.Name.Should().Be.EqualTo("Agent");
            response.Should().Be.OfType<BadRequestErrorMessageResult>();
        }

        [Test]
        public void ShouldNotAddNewFunctionsToBuiltInRole()
        {
            var functionOneId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var functionOne = new ApplicationFunction("FunctionOne");
            ApplicationFunctionRepository.Add(functionOne);
            var agentRole = new ApplicationRole { Name = "Agent", BuiltIn = true };
            ApplicationRoleRepository.Add(agentRole);
            Target.AddFunctions(roleId, new FunctionsForRoleInput { Functions = new Collection<Guid> { functionOneId } });

            agentRole.ApplicationFunctionCollection.Should().Be.Empty();
        }

        [Test]
        public void ShouldRemoveFunctionsFromRole()
        {
            var functionOneId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var functionOne = new ApplicationFunction("FunctionOne");
            functionOne.SetId(functionOneId);
            ApplicationFunctionRepository.Add(functionOne);
            var agentRole = new ApplicationRole { Name = "Agent" };
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
            var agentRole = new ApplicationRole { Name = "Agent", BuiltIn = true };
            agentRole.SetId(roleId);
            agentRole.AddApplicationFunction(functionOne);
            ApplicationRoleRepository.Add(agentRole);
            agentRole.AddApplicationFunction(functionOne);
            Target.RemoveFunction(roleId, functionOneId);

            agentRole.ApplicationFunctionCollection.Should().Contain(functionOne);
        }

        [Test]
        public void ShouldCopyExistingRole()
        {
            Target.Request = new HttpRequestMessage();
            var team = TeamFactory.CreateTeam("Team 1", "Paris");
            var roleId = Guid.NewGuid();
            var functionOne = new ApplicationFunction("FunctionOne");
            var agentRole = new ApplicationRole { Name = "Agent", DescriptionText = "Agent", BuiltIn = true, AvailableData = new AvailableData() };
            agentRole.AddApplicationFunction(functionOne);
            agentRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MySite;
            agentRole.AvailableData.AddAvailableBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
            agentRole.AvailableData.AddAvailableSite(team.Site);
            agentRole.AvailableData.AddAvailableTeam(team);
            ApplicationRoleRepository.Add(agentRole);

            Target.CopyExistingRole(roleId);

            ApplicationRoleRepository.LoadAll().Last().DescriptionText.Should().Be.EqualTo("Copy of " + agentRole.DescriptionText);
            ApplicationRoleRepository.LoadAll().Last().Name.Should().Be.EqualTo("Copyof" + agentRole.Name);
            ApplicationRoleRepository.LoadAll().Last().ApplicationFunctionCollection.Should().Have.Count.EqualTo(1);
            ApplicationRoleRepository.LoadAll().Last().AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(1);
            ApplicationRoleRepository.LoadAll().Last().AvailableData.AvailableSites.Should().Have.Count.EqualTo(1);
            ApplicationRoleRepository.LoadAll().Last().AvailableData.AvailableTeams.Should().Have.Count.EqualTo(1);
            ApplicationRoleRepository.LoadAll().Last().AvailableData.AvailableDataRange.Should().Be.EqualTo(AvailableDataRangeOption.MySite);
        }

        [Test]
        public void ShouldDeleteRole()
        {
            var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
            var target = new RolesController(roleRepository, null, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle"), null, null));
            var roleId = Guid.NewGuid();
            var role = new ApplicationRole();
            roleRepository.Stub(x => x.Load(roleId)).Return(role);
            target.Delete(roleId);
            roleRepository.AssertWasCalled(x => x.Remove(role));
        }

        [Test]
        public void ShouldNotDeleteBuiltInRole()
        {
            var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
            var target = new RolesController(roleRepository, null, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle"), null, null));
            var roleId = Guid.NewGuid();

            var role = new ApplicationRole { BuiltIn = true };
            roleRepository.Stub(x => x.Load(roleId)).Return(role);

            target.Delete(roleId);

            roleRepository.AssertWasNotCalled(x => x.Remove(role));
        }

        [Test]
        public void ShouldGetFunctionsAndAvailableDataForRole()
        {
            var agentRole = new ApplicationRole { Name = "Agent" };
            agentRole.SetId(Guid.NewGuid());
            agentRole.AddApplicationFunction(new ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage));
            agentRole.AvailableData = new AvailableData();
            agentRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyOwn;
            var simpleTeam = TeamFactory.CreateTeam("Team 1", "Paris");
            agentRole.AvailableData.AddAvailableTeam(simpleTeam);
            agentRole.AvailableData.AddAvailableSite(simpleTeam.Site);
            agentRole.AvailableData.AddAvailableBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
            ApplicationRoleRepository.Add(agentRole);
            dynamic result = Target.Get(agentRole.Id.GetValueOrDefault());

            ((string)result.Name).Should().Be.EqualTo(agentRole.Name);
            ((AvailableDataRangeOption)result.AvailableDataRange).Should().Be.EqualTo(AvailableDataRangeOption.MyOwn);
            ((ICollection<object>)result.AvailableTeams).Count.Should().Be.EqualTo(1);
            ((ICollection<object>)result.AvailableSites).Count.Should().Be.EqualTo(1);
            ((ICollection<object>)result.AvailableBusinessUnits).Count.Should().Be.EqualTo(1);
            ((ICollection<object>)result.AvailableFunctions).Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldRemoveFunctionWitChild()
        {
            var roleId = Guid.NewGuid();
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
            var agentRole = new ApplicationRole { Name = "Agent" };
            ApplicationRoleRepository.Add(agentRole);
            agentRole.AddApplicationFunction(parent);
            agentRole.AddApplicationFunction(child);
            agentRole.AddApplicationFunction(grandChild);
            Target.RemoveFunction(roleId, parentId);

            agentRole.ApplicationFunctionCollection.Count.Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldRemoveAvailableSiteWithTeamsFromRole()
        {
            var roleId = Guid.NewGuid();
            var team = TeamFactory.CreateSimpleTeam();
            var site = SiteFactory.CreateSimpleSite();
            var siteId = Guid.NewGuid();

            site.SetId(siteId);
            team.SetId(Guid.NewGuid());
            team.Site = site;
            var agentRole = new ApplicationRole { Name = "Agent", AvailableData = new AvailableData() };
            agentRole.AvailableData.AddAvailableTeam(team);

            agentRole.AvailableData.AddAvailableSite(site);
            ApplicationRoleRepository.Add(agentRole);

            Target.RemoveAvailableSite(roleId, site.Id.GetValueOrDefault());

            agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
            agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(0);
        }

        [Test]
        public void ShouldRemoveAvailableBusinessUnitSiteAndTeamFromRole()
        {
            var businessUnit = BusinessUnitFactory.CreateWithId("Bu 2");
            System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
                new TeleoptiIdentity("test", null, businessUnit, null), PersonFactory.CreatePerson());
            var roleId = Guid.NewGuid();
            var site = SiteFactory.CreateSimpleSite();

            businessUnit.SetId(site.BusinessUnit.Id);
            var agentRole = new ApplicationRole { Name = "Agent", AvailableData = new AvailableData() };
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

            Target.RemoveAvailableBusinessUnit(roleId, businessUnit.Id.GetValueOrDefault());

            agentRole.AvailableData.AvailableBusinessUnits.Should().Have.Count.EqualTo(0);
            agentRole.AvailableData.AvailableSites.Should().Have.Count.EqualTo(0);
            agentRole.AvailableData.AvailableTeams.Should().Have.Count.EqualTo(0);
        }
    }
}
