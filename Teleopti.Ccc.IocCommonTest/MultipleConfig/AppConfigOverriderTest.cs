﻿using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultipleConfig;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
{
	public class AppConfigOverriderTest
	{
		[Test]
		public void ShouldReturnNullForNonExistingKey()
		{
			var target = new ConfigOverrider(new FakeConfigReader(), new Dictionary<string, string>());
			target.AppConfig(RandomName.Make())
					.Should().Be.Null();
		}

		[Test]
		public void ShouldGetValueFromAppConfig()
		{
			var key = RandomName.Make();
			var value = RandomName.Make();
			var target = new ConfigOverrider(new FakeConfigReader(new Dictionary<string, string> {{key, value}}),
				new Dictionary<string, string>());
			target.AppConfig(key)
				.Should().Be.EqualTo(value);
		}

		[Test]
		public void ShouldGetValueFromOverrider()
		{
			var key = RandomName.Make();
			var value = RandomName.Make();
			var target =
				new ConfigOverrider(
					new FakeConfigReader(new Dictionary<string, string> {{key, value}}),
					new Dictionary<string, string> {{key, value}});
			target.AppConfig(key)
				.Should().Be.EqualTo(value);
		}
	}
}