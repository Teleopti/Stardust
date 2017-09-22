using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture]
    public class ExportSkillWizardPagesTest
    {
        private ExportSkillWizardPages _target;
        private ExportSkillModel exportSkillModel;
        private IExportAcrossBusinessUnitsSettingsProvider exportAcrossBusinessUnitsSettingsProvider;
        private IExportForecastToFileSettingsProvider exportForecastToFileSettingsProvider;
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            exportSkillModel = new ExportSkillModel(true,true);
            _unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            exportAcrossBusinessUnitsSettingsProvider = new ExportAcrossBusinessUnitsSettingsProvider(_unitOfWorkFactory,_repositoryFactory);
            exportForecastToFileSettingsProvider = new ExportForecastToFileSettingsProvider(_unitOfWorkFactory,_repositoryFactory);
            _target = new ExportSkillWizardPages(exportSkillModel, exportAcrossBusinessUnitsSettingsProvider, exportForecastToFileSettingsProvider);   
        }

        [Test]
        public void ShouldHaveNameInResourceFile()
        {
            Assert.IsNotNull(_target.Name);
        }

        [Test]
        public void ShouldHaveWindowTextInResourceFile()
        {
            Assert.IsNotNull(_target.WindowText);
        }

        [Test]
        public void ShouldReturnObject()
        {
            Assert.That(_target.CreateNewStateObj(), Is.Not.Null);
        }
    }
}
