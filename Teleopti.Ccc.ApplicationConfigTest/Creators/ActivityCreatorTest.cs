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
        private IGroupingActivity _groupingActivity;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;

            GroupingActivityCreator creator = new GroupingActivityCreator(_person, _sessionFactory);
            _groupingActivity = creator.Create("ga");
            _target = new ActivityCreator(_groupingActivity);
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
            Assert.AreEqual(_groupingActivity,activity.GroupingActivity);
        }
    }
}
