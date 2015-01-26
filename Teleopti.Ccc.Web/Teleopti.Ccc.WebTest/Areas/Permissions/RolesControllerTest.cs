using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class RolesControllerTest
	{
		[Test]
		public void ShouldCreateNewRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var dataRepository = MockRepository.GenerateMock<IAvailableDataRepository>();
			var target = new RolesController(roleRepository, null, dataRepository, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));
			target.Request = new HttpRequestMessage();

			bool itemAdded = false;
			roleRepository.Stub(x => x.Add(null)).IgnoreArguments().Callback<IApplicationRole>(item =>
			{
				item.SetId(Guid.NewGuid());
				itemAdded = true;
				return true;
			});

			target.Post(new NewRoleInput { Name = "Admin" });

			itemAdded.Should().Be.True();
		}

		[Test]
		public void ShouldGetAllRolesAvailable()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var dataRepository = MockRepository.GenerateMock<IAvailableDataRepository>();
			var target = new RolesController(roleRepository, null, dataRepository, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var agentRole = new ApplicationRole{Name = "Agent"};
			var adminRole = new ApplicationRole{Name = "Admin", BuiltIn = true};
			roleRepository.Stub(x => x.LoadAllApplicationRolesSortedByName())
				.Return(new List<IApplicationRole> {agentRole, adminRole});

			var result = target.Get();

			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldAddNewFunctionsToRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			var dataRepository = MockRepository.GenerateMock<IAvailableDataRepository>();
			var target = new RolesController(roleRepository, functionRepository, dataRepository, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var functionOneId = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var functionOne = new ApplicationFunction("FunctionOne");
			var agentRole = new ApplicationRole { Name = "Agent" };
			
			roleRepository.Stub(x => x.Get(roleId)).Return(agentRole);
			functionRepository.Stub(x => x.Load(functionOneId)).Return(functionOne);

			target.AddFunctions(roleId, new FunctionsForRoleInput{Functions = new Collection<Guid>{functionOneId}});

			agentRole.ApplicationFunctionCollection.Should().Contain(functionOne);
		}

		[Test]
		public void ShouldRenameRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var target = new RolesController(roleRepository, null, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var roleId = Guid.NewGuid();

			var agentRole = new ApplicationRole { Name = "Agent" };

			roleRepository.Stub(x => x.Get(roleId)).Return(agentRole);
			
			target.RenameRole(roleId,new RenameRoleInput{NewName = "Self service agent"});

			agentRole.Name.Should().Be.EqualTo("Self service agent");
		}

		[Test]
		public void ShouldNotRenameBuiltInRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var target = new RolesController(roleRepository, null, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var roleId = Guid.NewGuid();

			var agentRole = new ApplicationRole { Name = "Agent",BuiltIn = true};

			roleRepository.Stub(x => x.Get(roleId)).Return(agentRole);

			target.RenameRole(roleId, new RenameRoleInput { NewName = "Self service agent" });

			agentRole.Name.Should().Be.EqualTo("Agent");
		}

		[Test]
		public void ShouldNotAllowRenameToEmptyName()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var target = new RolesController(roleRepository, null, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var roleId = Guid.NewGuid();

			var agentRole = new ApplicationRole { Name = "Agent", BuiltIn = true };

			roleRepository.Stub(x => x.Get(roleId)).Return(agentRole);

			target.RenameRole(roleId, new RenameRoleInput { NewName = "" });

			agentRole.Name.Should().Be.EqualTo("Agent");
		}

		[Test]
		public void ShouldNotAddNewFunctionsToBuiltInRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			var target = new RolesController(roleRepository, functionRepository, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var functionOneId = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var functionOne = new ApplicationFunction("FunctionOne");
			var agentRole = new ApplicationRole { Name = "Agent",BuiltIn = true};

			roleRepository.Stub(x => x.Get(roleId)).Return(agentRole);
			functionRepository.Stub(x => x.Load(functionOneId)).Return(functionOne);

			target.AddFunctions(roleId, new FunctionsForRoleInput { Functions = new Collection<Guid> { functionOneId } });

			agentRole.ApplicationFunctionCollection.Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveFunctionsFromRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			var target = new RolesController(roleRepository, functionRepository, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var functionOneId = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var functionOne = new ApplicationFunction("FunctionOne");
			var agentRole = new ApplicationRole { Name = "Agent" };
			agentRole.AddApplicationFunction(functionOne);

			roleRepository.Stub(x => x.Get(roleId)).Return(agentRole);
			functionRepository.Stub(x => x.Load(functionOneId)).Return(functionOne);

			target.RemoveFunctions(roleId, new FunctionsForRoleInput { Functions = new Collection<Guid> { functionOneId } });

			agentRole.ApplicationFunctionCollection.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotRemoveFunctionsFromBuiltInRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			var target = new RolesController(roleRepository, functionRepository, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));

			var functionOneId = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var functionOne = new ApplicationFunction("FunctionOne");
			var agentRole = new ApplicationRole { Name = "Agent",BuiltIn = true};
			agentRole.AddApplicationFunction(functionOne);

			roleRepository.Stub(x => x.Get(roleId)).Return(agentRole);
			functionRepository.Stub(x => x.Load(functionOneId)).Return(functionOne);

			target.RemoveFunctions(roleId, new FunctionsForRoleInput { Functions = new Collection<Guid> { functionOneId } });

			agentRole.ApplicationFunctionCollection.Should().Contain(functionOne);
		}

		[Test]
		public void ShouldDeleteRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var target = new RolesController(roleRepository, null, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));
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
			var target = new RolesController(roleRepository, null, null, new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));
			var roleId = Guid.NewGuid();

			var role = new ApplicationRole {BuiltIn = true};
			roleRepository.Stub(x => x.Load(roleId)).Return(role);
			
			target.Delete(roleId);

			roleRepository.AssertWasNotCalled(x => x.Remove(role));
		}

		[Test]
		public void ShouldGetFunctionsAndAvailableDataForRole()
		{
			var roleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var dataRepository = MockRepository.GenerateMock<IAvailableDataRepository>();
			var target = new RolesController(roleRepository, null, dataRepository,new CurrentBusinessUnit(new FakeCurrentIdentity("Pelle")));
			
			var agentRole = new ApplicationRole { Name = "Agent" };
			agentRole.SetId(Guid.NewGuid());
			agentRole.AddApplicationFunction(new ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage));
			agentRole.AvailableData = new AvailableData();
			agentRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyOwn;
			var simpleTeam = TeamFactory.CreateTeam("Team 1","Paris");
			agentRole.AvailableData.AddAvailableTeam(simpleTeam);
			agentRole.AvailableData.AddAvailableSite(simpleTeam.Site);
			agentRole.AvailableData.AddAvailableBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			agentRole.AvailableData.AddAvailablePerson(
				PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2015, 1, 1), simpleTeam));
			roleRepository.Stub(x => x.Get(agentRole.Id.GetValueOrDefault())).Return(agentRole);

			dynamic result = target.Get(agentRole.Id.GetValueOrDefault());

			((string)result.Name).Should().Be.EqualTo(agentRole.Name);
			((AvailableDataRangeOption)result.AvailableDataRange).Should().Be.EqualTo(AvailableDataRangeOption.MyOwn);
			((ICollection<object>)result.AvailableTeams).Count.Should().Be.EqualTo(1);
			((ICollection<object>)result.AvailableSites).Count.Should().Be.EqualTo(1);
			((ICollection<object>)result.AvailableBusinessUnits).Count.Should().Be.EqualTo(1);
			((ICollection<object>)result.AvailablePeople).Count.Should().Be.EqualTo(1);
		}
	}
}
