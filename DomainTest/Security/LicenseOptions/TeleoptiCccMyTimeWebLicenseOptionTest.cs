using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[TestFixture]
	public class TeleoptiCccMyTimeWebLicenseOptionTest
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void VerifyEnable()
		{
			var target = new TeleoptiCccMyTimeWebLicenseOption();

			target.EnableApplicationFunctions(new List<IApplicationFunction>());
			var result = target.EnabledApplicationFunctions;
			
			Assert.AreEqual(7, result.Count);
		}

	}
}