using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.ApplicationConfigTest.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class ApplicationRoleCreatorTest
    {
        private ApplicationRoleCreator _target;
        private ISessionFactory _sessionFactory;
        private IPerson _person;
        private IBusinessUnit _businessUnit;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            _businessUnit = CreatorFactory.CreateBusinessUnit("BU", _person, _sessionFactory);

            _target = new ApplicationRoleCreator(_person, _businessUnit, _sessionFactory);
        }

        [Test]
        public void VerifyCanCreateRole()
        {
            IApplicationRole applicationRole = _target.Create("Role", "Description", false);
            Assert.IsFalse(applicationRole.BuiltIn);
            Assert.AreEqual("Role",applicationRole.Name);
            Assert.AreEqual("Description", applicationRole.DescriptionText);
            Assert.AreEqual(_businessUnit, applicationRole.BusinessUnit);
        }

        [Test]
        public void VerifyCanSaveRole()
        {
            const string applicationRoleName = "Master";
            const string description = "Description";

            IApplicationRole applicationRole = _target.Create(applicationRoleName, description, false);
            _target.Save(applicationRole);
        }
    }
}