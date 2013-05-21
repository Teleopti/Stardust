using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Budgeting;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCodeTest.Forecasting.SkillPages;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class BudgetGroupWizardPageTest
    {
        private BudgetGroupWizardPage _target;
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
            _target = new BudgetGroupWizardPage(_budgetGroup,_repositoryFactory,_unitOfWorkFactory);
        }

        [Test]
        public void VerifyCreateWithPlanningGroup()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_budgetGroup, _target.AggregateRootObject);
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(_budgetGroup.Name, _target.Name);
            Assert.IsNotEmpty(_target.WindowText);
        }
        [Test]
        public void VerifyDefaultConstructor()
        {
            _target = new BudgetGroupWizardPage(_repositoryFactory,_unitOfWorkFactory);
            Assert.IsNotNull(_target);
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
                var budgetGroupRepository = _target.RepositoryObject;
                Assert.IsNotNull(budgetGroupRepository);
            }
        }

        [Test]
        public void VerifyOnAfterSaveWorks()
        {
            IUnitOfWork uow = _mocks.DynamicMock<IUnitOfWork>();
            
            var savedTester = _mocks.StrictMock<ISavedTester>();
            var propertyPage1 = _mocks.StrictMock<IPropertyPage>();
            IList<IPropertyPage> pages = new List<IPropertyPage> { propertyPage1 };

            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.Twice();
                
                savedTester.ISavedTesterSaved(null, null);
                LastCall.IgnoreArguments().Repeat.Once();

                propertyPage1.Depopulate(null);
                LastCall.IgnoreArguments().Return(true).Repeat.Once();

                uow.PersistAll();
                LastCall.Repeat.Twice().Return(new List<IRootChangeInfo>());
            }
            using(_mocks.Playback())
            {
                _target.Initialize(pages, new LazyLoadingManagerWrapper());
                _target.Saved += savedTester.ISavedTesterSaved;
                _target.Save();
            }
        }

        [Test]
        public void VerifyCreateNewRoot()
        {
            var budgetGroup = _target.CreateNewRoot();
            Assert.IsNotNull(budgetGroup);
        }

    }
}

