using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	[PermissionsTest]
	public class ApplicationFunctionsControllerTest
	{
		public ApplicationFunctionsController Target;
		public FakeApplicationFunctionsToggleFilter ApplicationFunctionsToggleFilter;

		[Test]
		public void ShouldGetFunctionHierarchy()
		{
			var functionOne = new ApplicationFunction("Code1"){ FunctionDescription = "Test 1"};
			var functionTwo = new ApplicationFunction("Code2") { FunctionDescription = "Hello"};
			functionTwo.Parent = functionOne;
			ApplicationFunctionsToggleFilter.AddFakeFunction(functionOne, _ => true);
			var result = Target.GetAllFunctions();
			var resultFunctions = result.Single();
			resultFunctions.LocalizedFunctionDescription.Should().Be.EqualTo("Test 1");
			resultFunctions.ChildFunctions.Single().LocalizedFunctionDescription.Should().Be.EqualTo("Hello");
		}

		[Test]
		public void ShouldMarkNonLicensedFunctionsDifferently()
		{
			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			ApplicationFunctionsToggleFilter.AddFakeFunction(functionOne, _ => false);
			var result = Target.GetAllFunctions();
			var resultFunctions = result.Single();
			resultFunctions.IsDisabled.Should().Be.True();
		}

		[Test]
		public void ShouldMarkLicensedFunctionsEnabled()
		{
			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			ApplicationFunctionsToggleFilter.AddFakeFunction(functionOne, _ => true);
			var result = Target.GetAllFunctions();
			var resultFunctions = result.Single();
			resultFunctions.IsDisabled.Should().Be.False();
		}

		[Test]
		public void ShouldIgnoreHiddenFunctions()
		{
			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			var systemFunction = new SystemFunction(functionOne, _ => true);
			systemFunction.SetHidden();
			ApplicationFunctionsToggleFilter.AddFakeSystemFunction(systemFunction);
			var result = Target.GetAllFunctions();
			result.Should().Be.Empty();
		}
	}
}