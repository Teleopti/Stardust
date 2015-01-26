using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class ApplicationFunctionsControllerTest
	{
		[Test]
		public void ShouldGetFunctionHierarchy()
		{
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();

			licensedFunctionsProvider.Stub(x => x.LicensedFunctions("datasource"))
				.Return(new [] {new ApplicationFunction("Code1"), new ApplicationFunction("Code1/Code2")});

			var functionOne = new ApplicationFunction("Code1"){Parent = null,FunctionDescription = "Test 1"};
			functionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction>
				{
					functionOne,
					new ApplicationFunction("Code2") {Parent = functionOne, FunctionDescription = "Hello"}
				});

			var target = new ApplicationFunctionsController(functionRepository, licensedFunctionsProvider, new FakeCurrentDatasource("datasource"));
			dynamic result = target.GetAllFunctions();

			var resultFunctions = ((ICollection<ApplicationFunctionViewModel>)result.Functions).Single();
			resultFunctions.LocalizedFunctionDescription.Should().Be.EqualTo("Test 1");
			resultFunctions.ChildFunctions.Single().LocalizedFunctionDescription.Should().Be.EqualTo("Hello");
		}

		[Test]
		public void ShouldMarkNonLicensedFunctionsDifferently()
		{
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();

			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			functionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction> {functionOne});

			licensedFunctionsProvider.Stub(x => x.LicensedFunctions("datasource")).Return(new IApplicationFunction[] {});

			var target = new ApplicationFunctionsController(functionRepository, licensedFunctionsProvider, new FakeCurrentDatasource("datasource"));
			dynamic result = target.GetAllFunctions();

			var resultFunctions = ((ICollection<ApplicationFunctionViewModel>)result.Functions).Single();
			resultFunctions.IsDisabled.Should().Be.True();
		}

		[Test]
		public void ShouldMarkLicensedFunctionsEnabled()
		{
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();

			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			functionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction> { functionOne });

			licensedFunctionsProvider.Stub(x => x.LicensedFunctions("datasource")).Return(new [] { new ApplicationFunction("Code1")  });

			var target = new ApplicationFunctionsController(functionRepository, licensedFunctionsProvider, new FakeCurrentDatasource("datasource"));
			dynamic result = target.GetAllFunctions();

			var resultFunctions = ((ICollection<ApplicationFunctionViewModel>)result.Functions).Single();
			resultFunctions.IsDisabled.Should().Be.False();
		}
	}
}