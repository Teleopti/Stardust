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
		private TeleoptiCccVersion8LicenseOption _target;

		[SetUp]
		public void Setup()
		{
			_target = new TeleoptiCccVersion8LicenseOption();
		}

		[Test]
		public void VerifyEnable()
		{
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList.ToList();

			_target.EnableApplicationFunctions(inputList);
			IList<IApplicationFunction> resultList = _target.EnabledApplicationFunctions;
			var functions = resultList.Select(r => r.FunctionPath);

			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.Anywhere);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.IntradayReForecasting);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);
			functions.Should().Contain(DefinedRaptorApplicationFunctionPaths.MonthSchedule);
		}
	}
}