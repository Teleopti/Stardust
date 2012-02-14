using System.Globalization;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.ApplicationConfigTest.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class AvailableDataCreatorTest
    {
        private IPerson _person;
        private ISessionFactory _sessionFactory;
        private AvailableDataCreator _target;
        private IApplicationRole _applicationRole;
        private IBusinessUnit _businessUnit;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            _businessUnit = CreatorFactory.CreateBusinessUnit("BU", _person, _sessionFactory);
            _applicationRole = CreatorFactory.CreateApplicationRole(_person, _businessUnit, _sessionFactory);

            _target = new AvailableDataCreator(_person, _sessionFactory);
        }

        [Test]
        public void VerifyCanCreateAvailableData()
        {
            IAvailableData availableData = _target.Create(_applicationRole, AvailableDataRangeOption.Everyone);
            Assert.AreEqual(AvailableDataRangeOption.Everyone, availableData.AvailableDataRange);
            Assert.AreEqual(_applicationRole, availableData.ApplicationRole);
        }

        [Test]
        public void VerifyCanSaveAvailableData()
        {
            IAvailableData availableData = _target.Create(_applicationRole, AvailableDataRangeOption.Everyone);
            _target.Save(availableData);
        }
    }
}