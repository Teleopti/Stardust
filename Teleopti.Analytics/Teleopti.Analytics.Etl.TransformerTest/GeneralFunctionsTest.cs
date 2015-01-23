using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class GeneralFunctionsTest
    {
        private GeneralFunctions _target;
        private IGeneralInfrastructure _generalInfrastructure;

        [SetUp]
        public void Setup()
        {
						_generalInfrastructure = MockRepository.GenerateMock<IGeneralInfrastructure>();
						_target = new GeneralFunctions(_generalInfrastructure, "yadayada");
        }

        [Test]
        public void VerifyDataSourceValidList()
        {
	        _generalInfrastructure.Stub(x => x.GetDataSourceList(true, false)).Return(new List<IDataSourceEtl>());

					IList<IDataSourceEtl> list = _target.DataSourceValidList;
	        list.Count.Should().Be.EqualTo(0);
        }

        [Test]
        public void VerifyGetInitialLoadState()
        {
	        _generalInfrastructure.Stub(x => x.GetInitialLoadState()).Return(2);

	        var result = _target.GetInitialLoadState();
	        result.Should().Be.EqualTo(EtlToolStateType.Valid);
        }

        [Test]
        public void VerifyLoadNewDataSourcesFromAggregationDatabase()
        {
					_target.LoadNewDataSources();
					_generalInfrastructure.AssertWasCalled(x => x.LoadNewDataSourcesFromAggregationDatabase());
        }

        [Test]
        public void VerifyGetTimeZoneList()
        {
	        _generalInfrastructure.Stub(x => x.GetTimeZonesFromMart())
		        .Return(new List<ITimeZoneDim>())
		        .Repeat.AtLeastOnce();

	        var result = _target.GetTimeZoneList();
	        result.Should().Not.Be.Null();
        }

        [Test, SetUICulture("sv-SE")]
        public void ShouldTranslateDataSourceNameForLogObjectItemAll()
        {
					IList<IDataSourceEtl> originalDatasourceList = new List<IDataSourceEtl>
					{
						new DataSourceEtl(1, "ResourceKeyAliasForAll", 0, "UTC", 15, false)
					};

	        _generalInfrastructure.Stub(x => x.GetDataSourceList(true, true)).Return(originalDatasourceList);

	        var result = _target.DataSourceValidListIncludedOptionAll;

	        result.Should().Not.Be.Null();
	        result.Count.Should().Be.EqualTo(1);
	        result[0].DataSourceName.Should().Be.EqualTo(Resources.AllSelection);
        }

		[Test]
		public void ShouldLoadBaseConfigurationsCorrectly()
		{
			var baseConfig = new BaseConfiguration(1053, 15, "UTC", null);
			_generalInfrastructure.Stub(x => x.LoadBaseConfiguration()).Return(baseConfig);

			var baseConfiguration = _target.LoadBaseConfiguration();

			baseConfiguration.Should().Be.SameInstanceAs(baseConfig);
		}

		[Test]
		public void ShouldSaveBaseConfiguration()
		{
			IBaseConfiguration config = new BaseConfiguration(1053, 60, "UTC", null);
			_target.SaveBaseConfiguration(config);

			_generalInfrastructure.AssertWasCalled(x => x.SaveBaseConfiguration(config));
		}

		[Test]
		public void ShouldReturnIntervalLengthAlreadyInUse()
		{
			_generalInfrastructure.Stub(x => x.LoadRowsInDimIntervalTable()).Return(96);

			int? intervalLength = _target.LoadIntervalLengthInUse();
			intervalLength.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldReturnNullIfNotIntervalLengthAlreadyInUse()
		{
			_generalInfrastructure.Stub(x => x.LoadRowsInDimIntervalTable()).Return(0);

			int? intervalLength = _target.LoadIntervalLengthInUse();
			intervalLength.HasValue.Should().Be.False();
		}
    }
}