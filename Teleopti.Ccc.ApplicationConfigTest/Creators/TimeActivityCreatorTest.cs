using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.ApplicationConfigTest.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class TimeActivityCreatorTest
    {
        private TimeActivityCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;
        private IBusinessUnit _businessUnit;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            _businessUnit = CreatorFactory.CreateBusinessUnit("BU", _person, _sessionFactory);
            
            _target = new TimeActivityCreator(_person, _businessUnit, _sessionFactory);
        }

        [Test]
        public void VerifyCanCreateTimeActivity()
        {
            ITimeActivity timeActivity = _target.Create("Default");
            Assert.AreEqual(new Description("Default"), timeActivity.Description);
            Assert.AreEqual(ExtraTimeType.None, timeActivity.ExtraTimeType);
            Assert.AreEqual(InfluenceType.None, timeActivity.InfluenceType);
            Assert.AreEqual(false, timeActivity.InContractTime);
            Assert.AreEqual(_businessUnit, timeActivity.BusinessUnit);
        }

        [Test]
        public void VerifyCanSaveTimeActivity()
        {
            ITimeActivity timeActivity = _target.Create("Default");
            _target.Save(timeActivity);
        }
    }
}
