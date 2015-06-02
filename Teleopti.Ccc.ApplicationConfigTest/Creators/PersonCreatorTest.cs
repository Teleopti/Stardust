using System;
using System.Globalization;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.Domain.Time;
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
    public class PersonCreatorTest
    {
        private PersonCreator _target;
        private ISessionFactory _sessionFactory;
        private TimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _target = new PersonCreator(_sessionFactory);
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        [Test]
        public void VerifyCanCreatePerson()
        {
            const string databaseConverterClient = "DatabaseConverterClient";
            const string databaseConverter = "DatabaseConverter";
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(1099); //kn-IN
            
            IPerson person = _target.Create(databaseConverterClient,
                                            databaseConverter, cultureInfo, _timeZone);

            Assert.AreEqual(databaseConverterClient, person.Name.FirstName);
            Assert.AreEqual(databaseConverter, person.Name.LastName);
            Assert.AreEqual(cultureInfo, person.PermissionInformation.UICulture());
            Assert.AreEqual(cultureInfo, person.PermissionInformation.Culture());
        }

        [Test]
        public void VerifyCanSavePerson()
        {
            IPerson person = _target.Create("Joe1", "Blogs1", CultureInfo.GetCultureInfo(1053), _timeZone);
            _target.Save(person);
        }


        [Test]
        public void VerifyCanFetchPerson()
        {
            IPerson person = _target.Create("Joe3", "Blogs3", CultureInfo.GetCultureInfo(1053), _timeZone);
            _target.Save(person);

            Assert.IsNotNull(person.Id);
        }
    }
}