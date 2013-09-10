using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-10-24
    /// </remarks>
    [TestFixture]
    [Category("LongRunning")]
    public class BusinessUnitCreatorTest
    {
        private BusinessUnitCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;

            _target = new BusinessUnitCreator(_person,_sessionFactory);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanCreateBusinessUnit()
        {
            string businessUnitName = "MyBusinessUnit";
            IBusinessUnit businessUnit = _target.Create(businessUnitName);
            Assert.AreEqual(businessUnitName, businessUnit.Name);
        }

        [Test]
        public void VerifyCanSaveBusinessUnit()
        {
            IBusinessUnit businessUnit = _target.Create("MyBusinessUnit");
            _target.Save(businessUnit);
        }
    }
}