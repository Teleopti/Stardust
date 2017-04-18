using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class BudgetGroupPropertiesPageTest
    {
        private BudgetGroupPropertiesPage _budgetGroupPropertiesPage;
        private IBudgetGroup _budgetGroup;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _budgetGroup = new BudgetGroup();
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _budgetGroupPropertiesPage = new BudgetGroupPropertiesPage(_budgetGroup,_repositoryFactory,_unitOfWorkFactory);
        }

        [Test]
        public void VerifyCreateWithPlanningGroup()
        {
            Assert.IsNotNull(_budgetGroupPropertiesPage);
            Assert.AreEqual(_budgetGroup, _budgetGroupPropertiesPage.AggregateRootObject);
        }
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.IsEmpty(_budgetGroupPropertiesPage.Name);
            Assert.IsNotEmpty(_budgetGroupPropertiesPage.WindowText);
        }

        [Test]
        public void VerifyCreateNewRoot()
        {
            var budgetGroup = _budgetGroupPropertiesPage.CreateNewRoot();
            Assert.IsNotNull(budgetGroup);
        }

        [Test]
        public void VerifyGetRepositoryObject()
        {
            IUnitOfWork uow = _mocks.DynamicMock<IUnitOfWork>();
            IBudgetGroupRepository repository = _mocks.DynamicMock<IBudgetGroupRepository>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreateBudgetGroupRepository(uow)).Return(repository);
            }
            using(_mocks.Playback())
            {
                IRepository<IBudgetGroup> budgetGroupRepository = _budgetGroupPropertiesPage.RepositoryObject;
                Assert.IsNotNull(budgetGroupRepository);
            }
        }
    }
}
