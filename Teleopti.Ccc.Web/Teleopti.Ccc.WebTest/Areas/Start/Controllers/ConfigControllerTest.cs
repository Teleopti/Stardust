using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class ConfigControllerTest
	{
		private ConfigController _target;

		[Test]
		public void ShouldGetSomething()
		{
			_target = new ConfigController();
			_target.SharedSettings().Should().Not.Be.Null();
		}
	}

	
}