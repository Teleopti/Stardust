using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class GeneralFunctionsTest
    {
        private GeneralFunctions _target;
        private MockRepository _mocks;
        private IGeneralInfrastructure _generalInfrastructure;

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _generalInfrastructure = _mocks.StrictMock<IGeneralInfrastructure>();
            _target = new GeneralFunctions(_generalInfrastructure, "yadayada");
        }

        #endregion

        [Test]
        public void VerifyDataSourceValidList()
        {
            using (_mocks.Record())
            {
                Expect.On(_generalInfrastructure).Call(_generalInfrastructure.GetDataSourceList(true, false)).Return(
                    new List<IDataSourceEtl>());
            }
            using (_mocks.Playback())
            {
                IList<IDataSourceEtl> list = _target.DataSourceValidList;
                Assert.AreEqual(0, list.Count);
            }
        }

        [Test]
        public void VerifyNoPublicEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(GeneralFunctions)));
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyGetInitialLoadState()
        {
            using (_mocks.Record())
            {
                Expect.Call(_generalInfrastructure.GetInitialLoadState()).Return(2);
            }

            using (_mocks.Playback())
            {
                _target.GetInitialLoadState();
                Assert.AreEqual((EtlToolStateType)2, EtlToolStateType.Valid);
            }
        }

        [Test]
        public void VerifyLoadNewDataSourcesFromAggregationDatabase()
        {
            using (_mocks.Record())
            {
                _generalInfrastructure.LoadNewDataSourcesFromAggregationDatabase();
            }

            using (_mocks.Playback())
            {
                _target.LoadNewDataSources();
            }
        }

        [Test]
        public void VerifyGetTimeZoneList()
        {
            using (_mocks.Record())
            {
                Expect.Call(_generalInfrastructure.GetTimeZonesFromMart()).Return(new List<ITimeZoneDim>()).Repeat.
                    AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                IList<ITimeZoneDim> timeZones = _target.GetTimeZoneList();
                Assert.IsNotNull(timeZones);
            }
        }

        [Test, SetUICulture("sv-SE")]
        public void ShouldTranslateDataSourceNameForLogObjectItemAll()
        {
            using (_mocks.Record())
            {
                IList<IDataSourceEtl> originalDatasourceList = new List<IDataSourceEtl> { new DataSourceEtl(1, "ResourceKeyAliasForAll", 0, "UTC", 15, false) };
                Expect.Call(_generalInfrastructure.GetDataSourceList(true, true)).Return(originalDatasourceList);
            }
            using (_mocks.Playback())
            {
                IList<IDataSourceEtl> datasources = _target.DataSourceValidListIncludedOptionAll;
                
                Assert.IsNotNull(datasources);
                Assert.AreEqual(1, datasources.Count);
                Assert.AreEqual(Resources.AllSelection, datasources[0].DataSourceName);
            }
        }

		[Test]
		public void ShouldLoadBaseConfigurationsCorrectly()
		{
			var baseConfig = new BaseConfiguration(1053, 15, "UTC", null);
			_generalInfrastructure.Expect(x => x.LoadBaseConfiguration()).Return(baseConfig);

			var baseConfiguration = _target.LoadBaseConfiguration();

			baseConfiguration.Should().Be.SameInstanceAs(baseConfig);
		}

		[Test]
		public void ShouldSaveBaseConfiguration()
		{
			_generalInfrastructure = MockRepository.GenerateMock<IGeneralInfrastructure>();
			_target = new GeneralFunctions(_generalInfrastructure, "sdfsd");

			IBaseConfiguration config = new BaseConfiguration(1053, 60, "UTC", null);
			_target.SaveBaseConfiguration(config);

			_generalInfrastructure.AssertWasCalled(x => x.SaveBaseConfiguration(config));
		}

		[Test]
		public void ShouldReturnIntervalLengthAlreadyInUse()
		{
			_generalInfrastructure = MockRepository.GenerateMock<IGeneralInfrastructure>();
			_target = new GeneralFunctions(_generalInfrastructure, "sdfsd");

			_generalInfrastructure.Stub(x => x.LoadRowsInDimIntervalTable()).Return(96);

			int? intervalLength = _target.LoadIntervalLengthInUse();
			intervalLength.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldReturnNullIfNotIntervalLengthAlreadyInUse()
		{
			_generalInfrastructure = MockRepository.GenerateMock<IGeneralInfrastructure>();
			_target = new GeneralFunctions(_generalInfrastructure, "sdfsd");

			_generalInfrastructure.Stub(x => x.LoadRowsInDimIntervalTable()).Return(0);

			int? intervalLength = _target.LoadIntervalLengthInUse();
			intervalLength.HasValue.Should().Be.False();
		}
    }
}