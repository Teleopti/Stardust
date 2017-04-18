using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.WorkloadPages;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.WorkloadPages
{
    /// <summary>
    /// Tests for the page manager class for workload properties
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    [TestFixture]
    public class WorkloadPropertiesPagesTest
    {
        private WorkloadPropertiesPages target;
        private IWorkload _workload;
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
            _workload = WorkloadFactory.CreateWorkload(_skill);

            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();

            target = new WorkloadPropertiesPages(_workload,_repositoryFactory,_unitOfWorkFactory);
        }

        /// <summary>
        /// Verifies the type of the create with workload.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCreateWithWorkload()
        {
            Assert.IsNotNull(target);
            Assert.AreEqual(_workload, target.AggregateRootObject);
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
            target = new WorkloadPropertiesPages(_skill,_repositoryFactory,_unitOfWorkFactory);

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
            Assert.IsEmpty(target.Name);
            Assert.IsNotEmpty(target.WindowText);
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

        /// <summary>
        /// Verifies the get repository object.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-25
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
    }
}
