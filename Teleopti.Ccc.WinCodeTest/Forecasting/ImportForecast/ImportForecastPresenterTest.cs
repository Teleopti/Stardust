using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastPresenterTest
    {
        private MockRepository _mock;
        private IImportForecast _view;
        private ImportForecastModel _model;
        private ImportForecastPresenter _target;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
        
        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _view = _mock.DynamicMock<IImportForecast>();
            _unitOfWorkFactory = _mock.DynamicMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mock.DynamicMock<IRepositoryFactory>();
            ISkill tempSkill = SkillFactory.CreateSkill("test");
            _model = new ImportForecastModel(tempSkill, _repositoryFactory, _unitOfWorkFactory);
            _target = new ImportForecastPresenter(_view, _model);

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
            ISkill tempSkill = SkillFactory.CreateSkillWithWorkloadAndSources();
            ImportForecastModel tempModel = new ImportForecastModel(tempSkill, _repositoryFactory, _unitOfWorkFactory);
            _target = new ImportForecastPresenter(_view, tempModel);
            _target.PopulateWorkloadList();
            Assert.IsNotEmpty(_target.WorkloadList.ToList());

        }
    }
}
