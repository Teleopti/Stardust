using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.WorkloadPages;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.WorkloadPages
{
    /// <summary>
    /// Page manager class for workload wizard pages
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    [TestFixture]
    public class WorkloadWizardPagesTest
    {
        private WorkloadWizardPages target;
        private ISkill _skill;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("testSkill");

            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();

            target = new WorkloadWizardPages(_skill,_repositoryFactory,_unitOfWorkFactory);
        }

        /// <summary>
        /// Verifies the create with skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCreateWithSkill()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the default properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(UserTexts.Resources.WorkloadWizard, target.Name);
            Assert.IsNotEmpty(target.WindowText);
        }

        /// <summary>
        /// Verifies the get repository object.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyGetRepositoryObject()
        {
            IUnitOfWork uow = _mocks.DynamicMock<IUnitOfWork>();
            IWorkloadRepository repository = _mocks.DynamicMock<IWorkloadRepository>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreateWorkloadRepository(uow)).Return(repository);
            }
            using(_mocks.Playback())
            {
                IRepository<IWorkload> workloadRepository = target.RepositoryObject;
                Assert.IsNotNull(workloadRepository);
            }
        }

        /// <summary>
        /// Verifies the save works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifySaveWorks()
        {
            IPropertyPage propertyPage1 = _mocks.StrictMock<IPropertyPage>();
            IList<IPropertyPage> pages = new List<IPropertyPage>
                                             {
                propertyPage1
            };
            IUnitOfWork uow = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.Twice();
                propertyPage1.Depopulate(null);
                LastCall.IgnoreArguments().Return(true).Repeat.Once();

                uow.PersistAll();
                LastCall.Repeat.Twice().Return(new List<IRootChangeInfo>());
            }
            using(_mocks.Playback())
            {
                target.Initialize(pages, new LazyLoadingManagerWrapper());
                target.Save();
            }
        }

        /// <summary>
        /// Verifies the create new root.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCreateNewRoot()
        {
            IWorkload newWorkload = target.CreateNewRoot();

            Assert.IsNotNull(newWorkload);
        }
    }
}
