using System.Drawing;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class AbsenceCreatorTest
    {
        private AbsenceCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;
        private IGroupingAbsence _groupingAbsence;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            
            GroupingAbsenceCreator creator = new GroupingAbsenceCreator(_person, _sessionFactory);
            _groupingAbsence = creator.Create("ga");
            _target = new AbsenceCreator(_groupingAbsence);
        }

        [Test]
        public void VerifyCanCreateAbsence()
        {
            IAbsence absence = _target.Create(new Description("name", "shortName"), Color.BlanchedAlmond, 2, false);
            Assert.AreEqual(new Description("name", "shortName"), absence.Description);
            Assert.AreEqual(Color.BlanchedAlmond, absence.DisplayColor);
            Assert.AreEqual(2, absence.Priority);
            Assert.AreEqual(false, absence.Requestable);
            Assert.AreEqual(_groupingAbsence, absence.GroupingAbsence);
        }
    }
}
