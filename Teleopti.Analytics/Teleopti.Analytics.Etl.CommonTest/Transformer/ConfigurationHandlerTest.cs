using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class ConfigurationHandlerTest
	{
		private IGeneralFunctions _generalFunctions;
		private ConfigurationHandler _target;

		[SetUp]
		public void Setup()
		{
			_generalFunctions = MockRepository.GenerateMock<IGeneralFunctions>();
			_target = new ConfigurationHandler(_generalFunctions, new BaseConfigurationValidator());
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenAllSettingsAreValid()
		{
			var baseConfiguration = new BaseConfiguration(1053, 15, "UTC", false);

			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsInvalidWhenCultureSettingIsInvalid()
		{
			var baseConfiguration = new BaseConfiguration(-1, 15, "UTC", false);

			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.False();
		}

		[Test]
		public void ShouldReportConfigurationIsInvalidWhenIntervalLengthSettingIsInvalid()
		{
			var baseConfiguration = new BaseConfiguration(1053, 5, "UTC", false);
			
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.False();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength10IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 10, "UTC", false);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(10);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength15IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 15, "UTC", false);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(15);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength30IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 30, "UTC", false);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(30);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength60IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 60, "UTC", false);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(60);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsInvalidWhenTimeZoneSettingIsInvalid()
		{
			var baseConfiguration = new BaseConfiguration(1053, 15, "invalid time zone", false);

			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.False();
		}

		[Test]
		public void ShouldLoadAndCacheBaseConfiguration()
		{
			var loadedBaseConfiguration = new BaseConfiguration(1033, 15, "UTC", false);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(loadedBaseConfiguration).Repeat.Once();

			var baseConfiguration = _target.BaseConfiguration;
			baseConfiguration.Should().Be.SameInstanceAs(loadedBaseConfiguration);

			baseConfiguration = _target.BaseConfiguration;
			baseConfiguration.Should().Be.SameInstanceAs(loadedBaseConfiguration);
		}

		[Test]
		public void ShouldReturnIntervalLengthInUse()
		{
			_generalFunctions.Stub(x => x.LoadIntervalLengthInUse()).Return(30);

			_target.IntervalLengthInUse.Should().Be.EqualTo(30);
		}

		[Test]
		public void ShouldReturnNullForIntervalLengthInUse()
		{
			_generalFunctions.Stub(x => x.LoadIntervalLengthInUse()).Return(null);

			_target.IntervalLengthInUse.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldChangeBaseConfigurationAfterSavingNewConfiguration()
		{
			var originalConfig = new BaseConfiguration(null, null, null, false);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(originalConfig);
			var config = _target.BaseConfiguration;
			config.Should().Be.SameInstanceAs(originalConfig);

			var newConfig = new BaseConfiguration(1033, 30, "UTC", false);
			_target.SaveBaseConfiguration(newConfig);

			_target.BaseConfiguration.Should().Be.SameInstanceAs(newConfig);
		}

		[Test]
		public void ShouldReloadConfigurationAfterSetConnectionString()
		{
			var generalFunctions = new FakeGeneralFunctions();
			_target = new ConfigurationHandler(generalFunctions, new BaseConfigurationValidator());

			var originalConfig = new BaseConfiguration(null, null, null, false);
			generalFunctions.AddConfiguration("OldConnectionString", originalConfig);

			var newConfig = new BaseConfiguration(1033, 30, "UTC", false);
			generalFunctions.AddConfiguration("NewConnectionString", newConfig);

			_target.SetConnectionString("OldConnectionString");
			_target.BaseConfiguration.Should().Be.SameInstanceAs(originalConfig);

			_target.SetConnectionString("NewConnectionString");
			_target.BaseConfiguration.Should().Be.SameInstanceAs(newConfig);
		}
	}
}
