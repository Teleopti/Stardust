using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	//just test edge cases here - normal flow is tested by other test cases
	[TestFixture]
	public class TeleoptiCacheKeyTest
	{
		[Test]
		public void ShouldUseBaseClassIfNoEntity()
		{
			var target = new exposer();
			target.TheKey(3).Should().Be.EqualTo("3");
		}

		private class exposer : TeleoptiCacheKey
		{
			public string TheKey(object parameter)
			{
				return ParameterValue(parameter);
			}
		}
	}
}