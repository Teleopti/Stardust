using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.LicenseOptions
{
	[TestFixture]
	public class TeleoptiWfmVNextPilotLicenseOptionTest
	{
		[Test]
		public void VerifyEnable()
		{
			IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

			var target = new TeleoptiWfmVNextPilotLicenseOption();
			target.EnableApplicationFunctions(inputList);
			var resultList = target.EnabledApplicationFunctions;
			Assert.AreEqual(2, resultList.Length);
		}
	}
}