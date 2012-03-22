using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastPresenterTest
    {
        private MockRepository _mock;
        private ImportForecastModel _model;
        private ImportForecastPresenter _target;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IImportForecastsRepository _importForecastsRepository;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _mock.DynamicMock<IImportForecast>();
            _unitOfWorkFactory = _mock.DynamicMock<IUnitOfWorkFactory>();
            _importForecastsRepository = _mock.DynamicMock<IImportForecastsRepository>();
            var tempSkill = SkillFactory.CreateSkill("test");
            _model = new ImportForecastModel(tempSkill, _unitOfWorkFactory, _importForecastsRepository);
            _target = new ImportForecastPresenter(_model);
        }

        [Test]
        public void ShouldInitializeSkillName()
        {
            _target.GetSelectedSkillName();
            Assert.IsNotNull(_target.SkillName);
        }

        [Test]
        public void ShouldPopulateWorkloadList()
        {
            var tempSkill = SkillFactory.CreateSkillWithWorkloadAndSources();
            var tempModel = new ImportForecastModel(tempSkill, _unitOfWorkFactory, _importForecastsRepository);
            _target = new ImportForecastPresenter(tempModel);
            _target.PopulateWorkloadList();
            Assert.IsNotEmpty(_target.WorkloadList.ToList());
        }
    }
}
