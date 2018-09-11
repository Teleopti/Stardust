using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.LicenseOptions
{
	[TestFixture]
	class TeleoptiWfmChatBotLicenseOptionTest
	{
		[Test]
		public void VerifyEnable()
		{
			IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

			var target = new TeleoptiWfmChatBotLicenseOption();
			target.EnableApplicationFunctions(inputList);
			IList<IApplicationFunction> resultList = target.EnabledApplicationFunctions;
			Assert.AreEqual(1, resultList.Count);
		}
	}
}