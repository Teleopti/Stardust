using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core
{
	[TestFixture]
	public class IanaTimeZoneProviderTest
	{
		private IanaTimeZoneProvider target;

		[Test]
		public void ShouldNotThrowExceptionWhenMapNotExistTimezone()
		{
			target = new IanaTimeZoneProvider();
			string result = null ;
			Assert.DoesNotThrow(() => {
				result = target.WindowsToIana("nonExistTimezone");
			});
			result.Should().Be.EqualTo(string.Empty);
		}
	}
}