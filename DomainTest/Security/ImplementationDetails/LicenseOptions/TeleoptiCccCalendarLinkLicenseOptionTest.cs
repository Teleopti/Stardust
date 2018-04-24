﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.LicenseOptions
{
	public class TeleoptiCccCalendarLinkLicenseOptionTest
	{
		public void VerifyEnable()
		{
			var target = new TeleoptiCccCalendarLinkLicenseOption();

			target.EnableApplicationFunctions(new List<IApplicationFunction>());
			var result = target.EnabledApplicationFunctions;
			// don't know if this should be zero or all here
			Assert.AreEqual(0, result.Length);
		}

		[Test]
		public void ShouldNotEnablePayroll()
		{
			var target = new TeleoptiCccCalendarLinkLicenseOption();

			target.EnableApplicationFunctions(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions.ToList());

			target.EnabledApplicationFunctions.FirstOrDefault(
				f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.PayrollIntegration).Should().Be.Null();
		}
	}
}