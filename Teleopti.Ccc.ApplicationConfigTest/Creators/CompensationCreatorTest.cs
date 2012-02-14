using System.Drawing;
using System.Globalization;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.ApplicationConfigTest.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class CompensationCreatorTest
    {
        private CompensationCreator _target;
        private IPerson _person;
        private ISessionFactory _sessionFactory;
        private IBusinessUnit _businessUnit;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _person = SetupFixtureForAssembly.Person;
            _businessUnit = CreatorFactory.CreateBusinessUnit("BU", _person, _sessionFactory);
            
            _target = new CompensationCreator(_person, _businessUnit, _sessionFactory); 
        }

        [Test]
        public void VerifyCanCreateCompensation()
        {
            Compensation compensation =  _target.Create("SampleCompensation");
            Assert.AreEqual(CompensationType.None,compensation.CompensationType);
            Assert.AreEqual(new Description("SampleCompensation"), compensation.Description);
            Assert.AreEqual(Color.Empty, compensation.DisplayColor);
            Assert.AreEqual(ExtraTimeType.None, compensation.ExtraTimeType);
            Assert.AreEqual(0d, compensation.Factor);
            Assert.AreEqual(_businessUnit,compensation.BusinessUnit);
        }

        [Test]
        public void VerifyCanSaveCompensation()
        {
            Compensation compensation = _target.Create("SampleCompensation");
            _target.Save(compensation);
        }
    }
}