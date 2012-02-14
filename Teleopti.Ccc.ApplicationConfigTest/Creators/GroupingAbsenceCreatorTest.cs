using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class GroupingAbsenceCreatorTest
    {
        private GroupingAbsenceCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            _target = new GroupingAbsenceCreator(_person, _sessionFactory);
        }

        [Test]
        public void VerifyCanCreateGroupingAbsence()
        {
            IGroupingAbsence groupingAbsence = _target.Create("Default");
            Assert.AreEqual(new Description("Default"), groupingAbsence.Description);
            Assert.AreEqual("Default",groupingAbsence.Name);
            Assert.AreEqual(string.Empty,groupingAbsence.ShortName);
        }

        [Test]
        public void VerifyCanSaveGroupingAbsence()
        {
            IGroupingAbsence groupingAbsence = _target.Create("Default");
            _target.Save(groupingAbsence);
        }
    }
}
