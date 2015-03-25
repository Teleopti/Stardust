using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
{
	public class MultipleAppConfigReaderTest
	{
		[Test]
		public void ShouldReturnNullForNonExistingKey()
		{
			var file = Path.GetTempFileName();
			using (createFile(file))
			{
				var target = new MultipleAppConfigReader(new FakeAppConfigReader(), new ConfigOverrider(file));
				target.AppConfig(RandomName.Make())
					.Should().Be.Null();
			}
		}

		[Test]
		public void ShouldGetValueFromAppConfig()
		{
			var file = Path.GetTempFileName();
			using (createFile(file))
			{
				var key = RandomName.Make();
				var value = RandomName.Make();
				var target = new MultipleAppConfigReader(new FakeAppConfigReader(new Dictionary<string, string> {{key, value}}),
					new ConfigOverrider(file));
				target.AppConfig(key)
					.Should().Be.EqualTo(value);
			}
		}

		[Test]
		public void ShouldGetValueFromOverrider()
		{
			var file = Path.GetTempFileName();
			var key = RandomName.Make();
			var value = RandomName.Make();
			using (createFile(file, buildOverrideLine(key, value)))
			{
				var configOverrider = MockRepository.GenerateStub<IConfigOverrider>();
				configOverrider.Stub(x => x.AppSetting(key)).Return(value);
				var target =
					new MultipleAppConfigReader(
						new FakeAppConfigReader(new Dictionary<string, string> {{key, value + RandomName.Make()}}), new ConfigOverrider(file));
				target.AppConfig(key)
					.Should().Be.EqualTo(value);
			}
		}

		private static string buildOverrideLine(string key, string value)
		{
			return string.Format("<add key='{0}' value='{1}' />", key, value);
		}

		private static IDisposable createFile(string file, params string[] lines)
		{
			var fileContent = string.Format(@"
<appSettings>
{0}
</appSettings>
", string.Concat(lines));
			File.WriteAllText(file, fileContent);
			return new GenericDisposable(() => File.Delete(file));
		}
	}
}