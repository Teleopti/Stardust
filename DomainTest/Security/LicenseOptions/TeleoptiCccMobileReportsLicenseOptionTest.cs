﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[TestFixture]
	public class TeleoptiCccMobileReportsLicenseOptionTest
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void VerifyEnable()
		{
			var target = new TeleoptiCccMobileReportsLicenseOption();

			target.EnableApplicationFunctions(new List<IApplicationFunction>());
			var result = target.EnabledApplicationFunctions;

			Assert.AreEqual(1, result.Count);
		}
	}
}
