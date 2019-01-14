using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.SkillPages
{
    /// <summary>
    /// Tests for the page manager class for multisite skill properties
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    [TestFixture]
    public class MultisiteSkillPropertiesPagesTest
    {
        private MultisiteSkillPropertiesPages target;
        private ISkillType _skillType;
        private IMultisiteSkill _skill;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;

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
            _skill = SkillFactory.CreateMultisiteSkill("testSkill");
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            target = new MultisiteSkillPropertiesPages(_skill, _repositoryFactory, _unitOfWorkFactory);
        }

        /// <summary>
        /// Verifies the type of the create with multisite skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCreateWithMultisiteSkillType()
        {
            target = new MultisiteSkillPropertiesPages(_skillType,_repositoryFactory,_unitOfWorkFactory);

            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the create with multisite skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCreateWithMultisiteSkill()
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
			target = new MultisiteSkillPropertiesPages(_skillType, _repositoryFactory, _unitOfWorkFactory);
            IMultisiteSkill newSkill = target.CreateNewRoot();

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
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            }
            using(_mocks.Playback())
            {
                IRepository<IMultisiteSkill> skillRepository = target.RepositoryObject;
                Assert.IsNotNull(skillRepository);
            }
        }
    }
}
