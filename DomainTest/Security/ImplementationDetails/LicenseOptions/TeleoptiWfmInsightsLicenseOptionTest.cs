using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.LicenseOptions
{
	[TestFixture]
	public class TeleoptiWfmInsightsLicenseOptionTest
	{
		[Test]
		public void VerifyEnable()
		{
			var target = new TeleoptiWfmInsightsLicenseOption();
			target.EnableApplicationFunctions(new List<IApplicationFunction>());
			IList<IApplicationFunction> resultList = target.EnabledApplicationFunctions;
			Assert.AreEqual(3, resultList.Count);
		}
	}
}