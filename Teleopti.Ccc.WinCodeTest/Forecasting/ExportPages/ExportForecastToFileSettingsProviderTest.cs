using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;


namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture]
    public class ExportForecastToFileSettingsProviderTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IExportForecastToFileSettingsProvider _target;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _target = new ExportForecastToFileSettingsProvider(_unitOfWorkFactory, _repositoryFactory);
        }

        [Test]
        public void ShouldLoadSettings()
        {
            var exportForecastToFileSettings = _mocks.DynamicMock<IExportForecastToFileSettings>();
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(
                    _repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey
                        <IExportForecastToFileSettings>(
                            "ExportForecastToFileSettings", new ExportForecastToFileSettings())).Return(
                                exportForecastToFileSettings).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.ExportForecastToFileSettings, Is.EqualTo(exportForecastToFileSettings));
            }
        }

        [Test]
        public void ShouldSaveSettings()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            var settings = _mocks.DynamicMock<IExportForecastToFileSettings>();
            var globalSettingRepository = _mocks.StrictMock<ISettingDataRepository>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.Twice();
                Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork)).Return(globalSettingRepository).Repeat.Twice();
                Expect.Call(globalSettingRepository.FindValueByKey<IExportForecastToFileSettings>(
                                    "ExportForecastToFileSettings", new ExportForecastToFileSettings())).
                    Return(settings).IgnoreArguments();
                Expect.Call(globalSettingRepository.PersistSettingValue(null)).IgnoreArguments().Return(null);
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
            }
            using (_mocks.Playback())
            {
                _target.Save();
            }
        }

        [Test]
        public void ShouldTransformToSerializableModel()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var settings = new ExportForecastToFileSettings();
            
            using(_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey<IExportForecastToFileSettings>(
                        "ExportForecastToFileSettings", new ExportForecastToFileSettings())).Return(
                            settings).IgnoreArguments();
                Expect.Call(unitOfWork.Dispose);
            }
            using(_mocks.Playback())
            {
                _target.TransformToSerializableModel(new DateOnlyPeriod(new DateOnly(2012,08,06), new DateOnly(2012,08,10)));
                Assert.That(_target.ExportForecastToFileSettings.Period, Is.EqualTo(new DateOnlyPeriod(new DateOnly(2012, 08, 06), new DateOnly(2012, 08, 10))));
            }
        }

    }
}
