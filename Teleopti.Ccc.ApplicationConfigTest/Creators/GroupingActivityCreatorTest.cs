using System;
using System.Globalization;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class GroupingActivityCreatorTest
    {
        private GroupingActivityCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            _target = new GroupingActivityCreator(_person, _sessionFactory);
        }

        [Test]
        public void VerifyCanCreateGroupingActivity()
        {
            IGroupingActivity groupingActivity = _target.Create("Default");
            Assert.AreEqual(new Description("Default"), groupingActivity.Description);
            Assert.AreEqual("Default", groupingActivity.Name);
        }

        [Test]
        public void VerifyCanSaveGroupingActivity()
        {
            IGroupingActivity groupingActivity = _target.Create("Default");
            _target.Save(groupingActivity);            
        }
    }
}
