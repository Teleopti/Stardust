using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class SaveImportForecastFileCommandTest
    {
        private ISaveImportForecastFileCommand _target;
        private ImportForecastModel _importForecastModel;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IImportForecastsRepository _importForecastsRepository;
        private MockRepository _mocks;
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _importForecastModel = new ImportForecastModel {SelectedSkill = SkillFactory.CreateSkill("My Skill")};
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            _importForecastsRepository = _mocks.StrictMock<IImportForecastsRepository>();
            _target = new SaveImportForecastFileCommand(_importForecastModel, _unitOfWorkFactory,
                                                        _importForecastsRepository);
        }

        [Test]
        public void ShouldSaveForecastFileInDB()
        {
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_unitOfWork.PersistAll()).Return(null);
                Expect.Call(() => _importForecastsRepository.Add(null)).IgnoreArguments().Callback(
                    new Delegates.Function<bool, ForecastFile>(getForecastFile));
            }
            using (_mocks.Playback())
            {
                _target.Execute("Forecasting\\ImportForecast\\ValidFile.txt");
                _importForecastModel.FileId.Should().Not.Be.EqualTo(Guid.Empty);
            }
        }

        private bool getForecastFile(IForecastFile forecastFile)
        {
            forecastFile.SetId(Guid.NewGuid());
            return true;
        }
    }
}