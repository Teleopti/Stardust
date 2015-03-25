using System.Collections.Specialized;
using System.Configuration;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
{
	public class AppConfigReaderTest
	{
		[Test]
		public void ShouldReturnNullForNonExistingKey()
		{
			var target = new AppConfigReader(new FakeConfigReader(), new NoConfigOverrider());
			target.AppConfig(RandomName.Make())
				.Should().Be.Null();
		}

		[Test]
		public void ShouldGetValueFromAppConfig()
		{
			var key = RandomName.Make();
			var value = RandomName.Make();
			var target = new AppConfigReader(new FakeConfigReader {AppSettings = new NameValueCollection {{key, value}}}, new NoConfigOverrider());
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
			var target = new AppConfigReader(new FakeConfigReader { AppSettings = new NameValueCollection { { key, value + RandomName.Make() } } }, configOverrider);
			target.AppConfig(key)
				.Should().Be.EqualTo(value);
		}

		[Test]
		public void MakeItBehaveLikeASingleton()
		{
			AppConfigReader.Instance.Should().Be.Null();
			new AppConfigReader(new FakeConfigReader(), new NoConfigOverrider());
			AppConfigReader.Instance.Should().Not.Be.Null();
		}

		[TearDown]
		public void KillSingleton()
		{
			AppConfigReader.Destroy();
		}
	}
}