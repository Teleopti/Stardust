using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[TestFixture]
	public class TeleoptiCccVersion8LicenseOptionTest
	{
		[Test]
		public void VerifyEnable()
		{
			var target = new TeleoptiCccVersion8LicenseOption();
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList.ToList();

			target.EnableApplicationFunctions(inputList);
			IList<IApplicationFunction> resultList = target.EnabledApplicationFunctions;
			var functions = resultList.Select(r => r.FunctionPath);

			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.IntradayReForecasting);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
		}
	}
}