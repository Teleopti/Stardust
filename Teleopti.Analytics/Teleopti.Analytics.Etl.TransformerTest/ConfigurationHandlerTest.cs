using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
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
			_target = new ConfigurationHandler(_generalFunctions);
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenAllSettingsAreValid()
		{
			var baseConfiguration = new BaseConfiguration(1053, 15, "UTC", null);

			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsInvalidWhenCultureSettingIsInvalid()
		{
			var baseConfiguration = new BaseConfiguration(-1, 15, "UTC", null);

			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.False();
		}

		[Test]
		public void ShouldReportConfigurationIsInvalidWhenIntervalLengthSettingIsInvalid()
		{
			var baseConfiguration = new BaseConfiguration(1053, 5, "UTC", null);
			
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.False();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength10IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 10, "UTC", null);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(10);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength15IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 15, "UTC", null);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(15);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength30IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 30, "UTC", null);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(30);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsValidWhenIntervalLength60IsUsed()
		{
			var baseConfiguration = new BaseConfiguration(1053, 60, "UTC", null);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);
			_target.BaseConfiguration.IntervalLength.Should().Be.EqualTo(60);
			_target.IsConfigurationValid.Should().Be.True();
		}

		[Test]
		public void ShouldReportConfigurationIsInvalidWhenTimeZoneSettingIsInvalid()
		{
			var baseConfiguration = new BaseConfiguration(1053, 15, "invalid time zone", null);

			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(baseConfiguration);

			_target.IsConfigurationValid.Should().Be.False();
		}

		[Test]
		public void ShouldLoadAndCacheBaseConfiguration()
		{
			var loadedBaseConfiguration = new BaseConfiguration(1033, 15, "UTC", null);
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
			var originalConfig = new BaseConfiguration(null, null, null, null);
			_generalFunctions.Stub(x => x.LoadBaseConfiguration()).Return(originalConfig);
			var config = _target.BaseConfiguration;
			config.Should().Be.SameInstanceAs(originalConfig);

			var newConfig = new BaseConfiguration(1033, 30, "UTC", null);
			_target.SaveBaseConfiguration(newConfig);

			_target.BaseConfiguration.Should().Be.SameInstanceAs(newConfig);
		}
	}
}
