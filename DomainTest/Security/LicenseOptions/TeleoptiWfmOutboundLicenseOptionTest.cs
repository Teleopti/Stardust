using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;


namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[TestFixture]
	class TeleoptiWfmOutboundLicenseOptionTest
	{
		[Test]
		public void VerifyEnable()
		{
			IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

			var target = new TeleoptiWfmOutboundLicenseOption();
			target.EnableApplicationFunctions(inputList);
			IList<IApplicationFunction> resultList = target.EnabledApplicationFunctions;
			Assert.AreEqual(1, resultList.Count);
		}
	}
}
