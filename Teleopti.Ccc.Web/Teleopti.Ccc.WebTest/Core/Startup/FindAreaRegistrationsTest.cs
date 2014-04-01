using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	public class FindAreaRegistrationsTest
	{
		[Test]
		public void ShouldFindAnAreaRegistration()
		{
			//just check that at least one is found
			//if for some reason anywhereareregistration is removed, verify against another one
			new FindAreaRegistrations().AreaRegistrations().Select(x => x.GetType())
				.Should().Contain(typeof (AnywhereAreaRegistration));

		}
	}
}