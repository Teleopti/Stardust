using System.Collections.Specialized;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Conventions
{
	public class AppSettingsTest
	{
		[Test]
		public void NonMatchingPropertyModulesShouldNotCrash()
		{
			var configReader = MockRepository.GenerateStub<IConfigReader>();
			configReader.Stub(x => x.AppSettings).Return(new NameValueCollection());
			Assert.DoesNotThrow(() =>
				new testModuleWithNoAppSettingsProperties(configReader)
				);
		}

		[Test]
		public void ShouldSetString()
		{
			const string expected = "expected REsult";
			var config = setupConfig("thestringKEY", expected);

			var target = new testModule(config);
			target.appsettingsthestringKEY
				.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotSetNonConventionBasedProperty()
		{
			var config = setupConfig("something", "should not be set");

			var target = new testModule(config);
			target.Something
				.Should().Be.Null();
		}

		[Test]
		public void ShouldSetNonStringProperty()
		{
			const int expected = 34;
			var config = setupConfig("theintkey", expected.ToString());

			var target = new testModule(config);
			target.AppSettingsTheIntKey.Should().Be.EqualTo(expected);
		}

		[Test]
		public void MultiplePropertiesShouldBeSet()
		{
			var configReader = MockRepository.GenerateStub<IConfigReader>();
			var appSettings = new NameValueCollection { { "thestringkey", "1" },{"theintkey", "1"} };
			configReader.Stub(x => x.AppSettings).Return(appSettings);
			var target = new testModule(configReader);
			target.AppSettingsTheIntKey.Should().Be.EqualTo(1);
			target.appsettingsthestringKEY.Should().Be.EqualTo("1");
		}

		private static IConfigReader setupConfig(string key, string value)
		{
			var ret = MockRepository.GenerateStub<IConfigReader>();
			var appSettings = new NameValueCollection {{key, value}};
			ret.Stub(x => x.AppSettings).Return(appSettings);
			return ret;
		}


		private class testModule : TeleoptiModule 
		{
			public testModule(IConfigReader configReader) : base(configReader)
			{
			}

			public string Something { get; set; }
			public string appsettingsthestringKEY { get; set; }
			public int AppSettingsTheIntKey { get; set; }
		}

		private class testModuleWithNoAppSettingsProperties : TeleoptiModule
		{
			public testModuleWithNoAppSettingsProperties(IConfigReader configReader) : base(configReader)
			{
			}
		}
	}
}