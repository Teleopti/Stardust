using System.Drawing;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class ActivityCreatorTest
    {
        private ActivityCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;

            _target = new ActivityCreator();
        }

        [Test]
        public void VerifyCanCreateAbsence()
        {
            IActivity activity = _target.Create("name", new Description("name", "shortName"), Color.BlanchedAlmond, false, true);
            Assert.AreEqual("name", activity.Name);
            Assert.AreEqual(new Description("name", "shortName"), activity.Description);
            Assert.AreEqual(Color.BlanchedAlmond, activity.DisplayColor);
            Assert.AreEqual(false, activity.InReadyTime);
            Assert.AreEqual(true, activity.InContractTime);
        }
    }
}
