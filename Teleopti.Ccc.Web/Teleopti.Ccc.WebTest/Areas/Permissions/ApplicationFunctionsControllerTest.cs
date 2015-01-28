using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Permissions.Controllers;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class ApplicationFunctionsControllerTest
	{
		[Test]
		public void ShouldGetFunctionHierarchy()
		{
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionsToggleFilter>();
			
			var functionOne = new ApplicationFunction("Code1"){ FunctionDescription = "Test 1"};
			var functionTwo = new ApplicationFunction("Code2") { FunctionDescription = "Hello"};
			functionTwo.Parent = functionOne;

			functionRepository.Stub(x => x.FilteredFunctions())
				.Return(
					new AllFunctions(new Collection<SystemFunction>
					{
						new SystemFunction(functionOne, _ => true),
					}));

			var target = new ApplicationFunctionsController(functionRepository);
			var result = target.GetAllFunctions();

			var resultFunctions = result.Single();
			resultFunctions.LocalizedFunctionDescription.Should().Be.EqualTo("Test 1");
			resultFunctions.ChildFunctions.Single().LocalizedFunctionDescription.Should().Be.EqualTo("Hello");
		}

		[Test]
		public void ShouldMarkNonLicensedFunctionsDifferently()
		{
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionsToggleFilter>();
			
			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			functionRepository.Stub(x => x.FilteredFunctions())
				.Return(new AllFunctions(new Collection<SystemFunction>{new SystemFunction(functionOne, _ => false)}));

			var target = new ApplicationFunctionsController(functionRepository);
			var result = target.GetAllFunctions();

			var resultFunctions = result.Single();
			resultFunctions.IsDisabled.Should().Be.True();
		}

		[Test]
		public void ShouldMarkLicensedFunctionsEnabled()
		{
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionsToggleFilter>();
			
			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			functionRepository.Stub(x => x.FilteredFunctions())
				.Return(new AllFunctions(new Collection<SystemFunction>{new SystemFunction(functionOne,_ => true)}));

			var target = new ApplicationFunctionsController(functionRepository);
			var result = target.GetAllFunctions();

			var resultFunctions = result.Single();
			resultFunctions.IsDisabled.Should().Be.False();
		}

		[Test]
		public void ShouldIgnoreHiddenFunctions()
		{
			var functionRepository = MockRepository.GenerateMock<IApplicationFunctionsToggleFilter>();

			var functionOne = new ApplicationFunction("Code1") { Parent = null, FunctionDescription = "Test 1" };
			var systemFunction = new SystemFunction(functionOne, _ => true);
			systemFunction.SetHidden();
			functionRepository.Stub(x => x.FilteredFunctions())
				.Return(new AllFunctions(new Collection<SystemFunction> { systemFunction }));

			var target = new ApplicationFunctionsController(functionRepository);
			var result = target.GetAllFunctions();

			result.Should().Be.Empty();
		}
	}
}