using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.SkillPages;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.SkillPages
{
    /// <summary>
    /// Tests for the page manager class for skill wizard pages
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    [TestFixture]
    public class SkillWizardPagesTest
    {
        private SkillWizardPages target;
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
            _skillType = SkillTypeFactory.CreateSkillType();
            _skill = SkillFactory.CreateSkill("testSkill");

            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();

            target = new SkillWizardPages(_skill,_repositoryFactory,_unitOfWorkFactory);
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
            target = new SkillWizardPages(_skillType,_repositoryFactory,_unitOfWorkFactory);

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
        /// Verifies the set default skill properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        [Test]
        public void VerifySetDefaultSkillProperties()
        {
            Skill skill = new Skill("name", "desc", Color.FromArgb(0), 15, _skillType);
            SkillWizardPages.SetSkillDefaultSettings(skill);

            Assert.AreEqual(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone.Id,
                            skill.TimeZone.Id);
            Assert.IsNull(skill.Activity);
            Assert.AreEqual(7,skill.TemplateWeekCollection.Count);

            ServiceAgreement defaultSA = ServiceAgreement.DefaultValues();
            foreach (ISkillDayTemplate skillDayTemplate in skill.TemplateWeekCollection.Values)
            {
                Assert.AreEqual(1, skillDayTemplate.TemplateSkillDataPeriodCollection.Count);
                ITemplateSkillDataPeriod skillDataPeriod = skillDayTemplate.TemplateSkillDataPeriodCollection[0];
                Assert.AreEqual(defaultSA.ServiceLevel, skillDataPeriod.ServiceAgreement.ServiceLevel);
                Assert.AreEqual(defaultSA.MinOccupancy, skillDataPeriod.ServiceAgreement.MinOccupancy);
                Assert.AreEqual(defaultSA.MaxOccupancy, skillDataPeriod.ServiceAgreement.MaxOccupancy);
                Assert.AreEqual(new SkillPersonData(), skillDataPeriod.SkillPersonData);
            }
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
            Assert.AreEqual(_skill.Name, target.Name);
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

        /// <summary>
        /// Verifies the on after save works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyOnAfterSaveWorks()
        {
            ISavedTester savedTester = _mocks.StrictMock<ISavedTester>();
            IPropertyPage propertyPage1 = _mocks.StrictMock<IPropertyPage>();
            IList<IPropertyPage> pages = new List<IPropertyPage>
                                             {
                propertyPage1
            };

            IUnitOfWork uow = _mocks.DynamicMock<IUnitOfWork>();
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
                target.Initialize(pages, new LazyLoadingManagerWrapper());
                target.Saved += savedTester.ISavedTesterSaved;
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
            ISkill newSkill = target.CreateNewRoot();
            Assert.IsNotNull(newSkill);
            Assert.AreEqual("Skill type", newSkill.SkillType.Description.Name);
            Assert.AreEqual(ForecastSource.InboundTelephony,newSkill.SkillType.ForecastSource);
        }



        /// <summary>
        /// Verifies the type of the create new root with preselected skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCreateNewRootWithPreSelectedSkillType()
        {
            target = new SkillWizardPages(_skillType,_repositoryFactory,_unitOfWorkFactory);
            ISkill newSkill = target.CreateNewRoot();
            Assert.IsNotNull(newSkill);
            Assert.AreEqual(_skillType, newSkill.SkillType);
        }
    }


    /// <summary>
    /// Interface for testing the Saved event handler
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-17
    /// </remarks>
    public interface ISavedTester
    {
        /// <summary>
        /// Handles the Saved event of the ISavedTester control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard.AfterSavedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        void ISavedTesterSaved(object sender, AfterSavedEventArgs e);
    }
}
