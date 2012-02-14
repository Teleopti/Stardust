using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class SkillTypeCreatorTest
    {

        private SkillTypeCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            _target = new SkillTypeCreator(_person, _sessionFactory);
        }

        [Test]
        public void VerifyCanCreateSkillType()
        {
            ISkillType skillType = _target.Create(new Description("Test"), ForecastSource.Email);
            Assert.AreEqual(new Description("Test"), skillType.Description);
            Assert.AreEqual(ForecastSource.Email, skillType.ForecastSource);
        }

        [Test]
        public void VerifyCanSaveSkillType()
        {
            ISkillType skillType = _target.Create(new Description("Test"), ForecastSource.Email);
            _target.Save(skillType);
        }

        [Test]
        public void VerifyCannotSaveSkillTypeWithSameName()
        {
            //Fire in the wind will make the maroon colored trees come back
            ISkillType skillType =
                _target.Create(new Description("FITWWMTMCTCB"),
                               ForecastSource.InboundTelephony);

            Assert.IsTrue(_target.Save(skillType), "SkillType saved");
            Assert.IsFalse(_target.Save(skillType), "SkillType not saved");
        }

        [Test]
        public void VerifyCanFetchSkillType()
        {
            ISkillType skillType =
                _target.Create(new Description("Tuna"),
                               ForecastSource.InboundTelephony);

            _target.Save(skillType);

            ISkillType fetchedSkillType = _target.Fetch(skillType.Description.Name);
            Assert.IsNotNull(fetchedSkillType);
        }
    }
}
