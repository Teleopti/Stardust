using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Matrix
{
	[TestFixture]
	public class ApplicationFunctionResolverTest
	{
		private IFunctionsForRoleProvider _functionsForRoleProvider;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private ApplicationFunctionResolver _target;
		private MatrixPermissionHolder _matrixPermissionHolder;

		[SetUp]
		public void Setup()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			var team = TeamFactory.CreateSimpleTeam();
			team.SetId(Guid.NewGuid());
			_matrixPermissionHolder = new MatrixPermissionHolder(person, team, true);
			_functionsForRoleProvider = MockRepository.GenerateMock<IFunctionsForRoleProvider>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_target = new ApplicationFunctionResolver(_functionsForRoleProvider);
		}

		[Test]
		public void ShouldReturnMatrixReportFromOneRole()
		{
			var matrixFunction = createMatrixFunction("report");
			var role = ApplicationRoleFactory.CreateRole("role", "role");
			role.AddApplicationFunction(matrixFunction);
			role.AddApplicationFunction(createNonMatrixFunction());

			_functionsForRoleProvider.Stub(x => x.AvailableFunctions(role, _unitOfWorkFactory))
				.Return(role.ApplicationFunctionCollection);

			var result = _target.ResolveApplicationFunction(new HashSet<MatrixPermissionHolder> { _matrixPermissionHolder }, role, _unitOfWorkFactory);

			result.Count.Should().Be.EqualTo(1);
			var report1Permissions = result.First();
			report1Permissions.ApplicationFunction.Should().Be.SameInstanceAs(matrixFunction);
			report1Permissions.IsMy.Should().Be.EqualTo(_matrixPermissionHolder.IsMy);
			report1Permissions.Person.Should().Be.SameInstanceAs(_matrixPermissionHolder.Person);
			report1Permissions.Team.Should().Be.SameInstanceAs(_matrixPermissionHolder.Team);
		}

		[Test]
		public void ShouldReturnMatrixReportsFromTwoRoles()
		{
			var report1 = createMatrixFunction("report_1");
			var report2 = createMatrixFunction("report_2");
			var role1 = ApplicationRoleFactory.CreateRole("role_1", "role_1");
			var role2 = ApplicationRoleFactory.CreateRole("role_2", "role_2");
			role1.AddApplicationFunction(report1);
			role2.AddApplicationFunction(report2);

			_functionsForRoleProvider.Stub(x => x.AvailableFunctions(role1, _unitOfWorkFactory))
				.Return(role1.ApplicationFunctionCollection);
			var reportsFromRole1 = _target.ResolveApplicationFunction(new HashSet<MatrixPermissionHolder> { _matrixPermissionHolder }, role1, _unitOfWorkFactory);
			reportsFromRole1.Count.Should().Be.EqualTo(1);
			reportsFromRole1.First().ApplicationFunction.Should().Be.SameInstanceAs(report1);


			_functionsForRoleProvider.Stub(x => x.AvailableFunctions(role2, _unitOfWorkFactory))
				.Return(role2.ApplicationFunctionCollection);
			var reportsFromRole2 = _target.ResolveApplicationFunction(new HashSet<MatrixPermissionHolder> { _matrixPermissionHolder }, role2, _unitOfWorkFactory);
			reportsFromRole2.Count.Should().Be.EqualTo(2);
			reportsFromRole2.First().Satisfy(x => x.ApplicationFunction.Equals(report1) || x.ApplicationFunction.Equals(report2));
			reportsFromRole2.Last().Satisfy(x => x.ApplicationFunction.Equals(report1) || x.ApplicationFunction.Equals(report2));

		}

		private IApplicationFunction createMatrixFunction(string name)
		{
			return ApplicationFunctionFactory.CreateApplicationFunction(name, createRootFunction(),
				DefinedForeignSourceNames.SourceMatrix, Guid.NewGuid().ToString());
		}

		private IApplicationFunction createNonMatrixFunction()
		{
			return ApplicationFunctionFactory.CreateApplicationFunction("not_a_report", createRootFunction(),
				DefinedForeignSourceNames.SourceRaptor, Guid.NewGuid().ToString());
		}

		private IApplicationFunction createRootFunction()
		{
			return
				ApplicationFunctionFactory.CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);
		}
	}
}