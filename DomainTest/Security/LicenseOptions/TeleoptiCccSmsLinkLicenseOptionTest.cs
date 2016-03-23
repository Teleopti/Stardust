using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), TestFixture]
	public class TeleoptiCccSmsLinkLicenseOptionTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void VerifyEnable()
		{
			var target = new TeleoptiCccSmsLinkLicenseOption();

			target.EnableApplicationFunctions(new List<IApplicationFunction>());
			var result = target.EnabledApplicationFunctions;
			// don't know if this should be zero or all here
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void ShouldNotEnablePayroll()
		{
			var target = new TeleoptiCccSmsLinkLicenseOption();

			target.EnableApplicationFunctions(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions.ToList());

			target.EnabledApplicationFunctions.FirstOrDefault(
				f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.PayrollIntegration).Should().Be.Null();
		}
	}
}