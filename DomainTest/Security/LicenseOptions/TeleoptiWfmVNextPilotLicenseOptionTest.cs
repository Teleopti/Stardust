using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
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
			Assert.AreEqual(9, resultList.Count);
		}
	}
}