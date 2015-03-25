using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
{
	public class MultipleAppConfigReaderTest
	{
		[Test]
		public void ShouldReturnNullForNonExistingKey()
		{
			var target = new MultipleAppConfigReader(new FakeAppConfigReader(), new NoConfigOverrider());
			target.AppConfig(RandomName.Make())
				.Should().Be.Null();
		}

		[Test]
		public void ShouldGetValueFromAppConfig()
		{
			var key = RandomName.Make();
			var value = RandomName.Make();
			var target = new MultipleAppConfigReader(new FakeAppConfigReader(new Dictionary<string, string>{{key, value}}), new NoConfigOverrider());
			target.AppConfig(key)
				.Should().Be.EqualTo(value);
		}

		[Test]
		public void ShouldGetValueFromOverrider()
		{
			var key = RandomName.Make();
			var value = RandomName.Make();
			var configOverrider = MockRepository.GenerateStub<IConfigOverrider>();
			var appSettingsSection = new AppSettingsSection();
			appSettingsSection.Settings.Add(key, value);
			configOverrider.Stub(x => x.AppSettings()).Return(appSettingsSection);
			var target = new MultipleAppConfigReader(new FakeAppConfigReader(new Dictionary<string, string>{ { key, value + RandomName.Make() } }), configOverrider);
			target.AppConfig(key)
				.Should().Be.EqualTo(value);
		}
	}
}