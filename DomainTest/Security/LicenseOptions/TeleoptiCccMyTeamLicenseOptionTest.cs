﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[TestFixture]
	public class TeleoptiCccMyTeamLicenseOptionTest
	{
		[Test]
		public void VerifyEnable()
		{
			var target = new TeleoptiCccMyTeamLicenseOption();

			IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

			target.EnableApplicationFunctions(inputList);
			IList<IApplicationFunction> resultList = target.EnabledApplicationFunctions;
			Assert.AreEqual(16, resultList.Count);
		}
	}
}