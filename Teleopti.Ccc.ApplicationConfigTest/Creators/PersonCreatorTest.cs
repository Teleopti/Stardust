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
        private ICccTimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _target = new PersonCreator(_sessionFactory);
            _timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        [Test]
        public void VerifyCanCreatePerson()
        {
            const string databaseConverterClient = "DatabaseConverterClient";
            const string databaseConverter = "DatabaseConverter";
            const string password = "byseashare10";
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(1099); //kn-IN
            
            IPerson person = _target.Create(databaseConverterClient,
                                            databaseConverter,
                                            databaseConverter,
                                            password, cultureInfo, _timeZone);

            Assert.AreEqual(databaseConverterClient, person.Name.FirstName);
            Assert.AreEqual(databaseConverter, person.Name.LastName);
            Assert.AreEqual(person.CreatedBy, person);
            Assert.IsNotNull(person.CreatedOn);
            Assert.AreEqual(cultureInfo, person.PermissionInformation.UICulture());
            Assert.AreEqual(cultureInfo, person.PermissionInformation.Culture());
        }

        [Test]
        public void VerifyCanSavePerson()
        {
            IPerson person = _target.Create("Joe1", "Blogs1", "o1", "g1", CultureInfo.GetCultureInfo(1053), _timeZone);
            Assert.IsTrue(_target.Save(person),"Person Saved");
        }

        [Test]
        public void VerifyCannotSavePersonWithSameName()
        {
            IPerson person = _target.Create("Joe2", "Blogs2", "o2", "g2", CultureInfo.GetCultureInfo(1053), _timeZone);
            Assert.IsTrue(_target.Save(person),"Person saved");
            Assert.IsFalse(_target.Save(person),"Person not saved");
        }

        [Test]
        public void VerifyCanFetchPerson()
        {
            IPerson person = _target.Create("Joe3", "Blogs3", "o3", "g3", CultureInfo.GetCultureInfo(1053), _timeZone);
            _target.Save(person);

            IPerson fetchedPerson =
                _target.FetchPerson(person.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName);
            Assert.IsNotNull(fetchedPerson);
        }
    }
}