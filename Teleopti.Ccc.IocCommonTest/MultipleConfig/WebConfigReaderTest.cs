using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
{
	[TestFixture]
	public class WebConfigReaderTest
	{
		[Test]
		public void ShouldGetNullValueWhenNoSettingAvailable()
		{
			var target = new WebConfigReader(() => new WebSettings {Settings = new Dictionary<string, string>()});
			target.AppConfig(RandomName.Make()).Should().Be.Null();
		}

		[Test]
		public void ShouldGetValueFromWebSettings()
		{
			var key = RandomName.Make();
			var value = RandomName.Make();
			var target = new WebConfigReader(() =>
			{
				var settings = new WebSettings { Settings = new Dictionary<string, string>() };
				settings.Settings[key] = value;
				return settings;
			});
			target.AppConfig(key).Should().Be(value);
		}

		[Test]
		public void ShouldReturnNullIfConnectionStringDoesNotExist()
		{
			new WebConfigReader(() => new WebSettings()).ConnectionString("hej micke").Should().Be.Null();
		}
	}
}
