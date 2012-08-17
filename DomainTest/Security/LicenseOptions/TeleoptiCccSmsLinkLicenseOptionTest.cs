using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[TestFixture]
	public class TeleoptiCccSmsLinkLicenseOptionTest
	{
		[Test]
		public void VerifyEnable()
		{
			var target = new TeleoptiCccSmsLinkLicenseOption();

			target.EnableApplicationFunctions(new List<IApplicationFunction>());
			var result = target.EnabledApplicationFunctions;
			// don't know if this should be zero or all here
			Assert.AreEqual(0, result.Count);
		}
	}
}