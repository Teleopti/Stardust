using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Logon.License
{
	[DomainTest]
	[DefaultData]
	public class LincensedFunctionsTest
	{
		public IApplicationFunctionsProvider ApplicationFunctions;

		[Test]
		public void ShouldEnableModifyAdherence()
		{
			var actual = ApplicationFunctions.AllFunctions();

			actual.FindByFunctionPath(DefinedRaptorApplicationFunctionPaths.ModifyAdherence)
				.IsLicensed.Should().Be.True();
		}

		[Test]
		public void ShouldEnableManageRealTimeAdherence()
		{
			var actual = ApplicationFunctions.AllFunctions();

			actual.FindByFunctionPath(DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence)
				.IsLicensed.Should().Be.True();
		}

		[Test]
		public void ShouldEnableIntradayRealTimeAdherence()
		{
			var actual = ApplicationFunctions.AllFunctions();

			actual.FindByFunctionPath(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence)
				.IsLicensed.Should().Be.True();
		}

		[Test]
		public void ShouldEnableRealTimeAdherenceOverview()
		{
			var actual = ApplicationFunctions.AllFunctions();

			actual.FindByFunctionPath(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				.IsLicensed.Should().Be.True();
		}
	}
}