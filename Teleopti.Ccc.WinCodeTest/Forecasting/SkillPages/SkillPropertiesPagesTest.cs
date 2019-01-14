using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.SkillPages
{
    /// <summary>
    /// Tests for the page manager class for skill properties
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    [TestFixture]
    public class SkillPropertiesPagesTest
    {
        private SkillPropertiesPages target;
        private ISkillType _skillType;
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
            _skillType = SkillTypeFactory.CreateSkillTypePhone();
            _skill = SkillFactory.CreateSkill("testSkill");

            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();

            target = new SkillPropertiesPages(_skill,_repositoryFactory,_unitOfWorkFactory, new StaffingCalculatorServiceFacade());
        }

        /// <summary>
        /// Verifies the type of the create with skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCreateWithSkillType()
        {
            target = new SkillPropertiesPages(_skillType,_repositoryFactory,_unitOfWorkFactory, new StaffingCalculatorServiceFacade());

            Assert.IsNotNull(target);
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
            Assert.AreEqual(_skill, target.AggregateRootObject);
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
            ISkill newSkill = target.CreateNewRoot();

            Assert.IsNotNull(newSkill);
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
            ISkillRepository repository = _mocks.DynamicMock<ISkillRepository>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreateSkillRepository(uow)).Return(repository);
            }
            using(_mocks.Playback())
            {
                IRepository<ISkill> skillRepository = target.RepositoryObject;
                Assert.IsNotNull(skillRepository);
            }
        }
    }
}
