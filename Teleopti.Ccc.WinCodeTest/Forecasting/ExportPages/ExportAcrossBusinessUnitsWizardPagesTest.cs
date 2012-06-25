using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ExportAcrossBusinessUnitsWizardPagesTest
    {
        private ExportSkillWizardPages _target;
        private ExportSkillModel _exportSkillModel;
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IExportAcrossBusinessUnitsSettingsProvider _exportAcrossBusinessUnitsSettingsProvider;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _exportSkillModel = new ExportSkillModel();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _exportAcrossBusinessUnitsSettingsProvider = new ExportAcrossBusinessUnitsSettingsProvider(_unitOfWorkFactory, _repositoryFactory);
            _target = new ExportSkillWizardPages(_exportSkillModel, _exportAcrossBusinessUnitsSettingsProvider);
        }

        [Test]
        public void VerifyCreated()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_exportSkillModel, _target.StateObj);
        }

        [Test]
        public void ShouldCreateNewAggregateRoot()
        {
            _target = new ExportSkillWizardPages(_exportSkillModel, _exportAcrossBusinessUnitsSettingsProvider);
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.IsNotEmpty(_target.WindowText);
        }


    }
}
