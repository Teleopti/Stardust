using System;
using System.Globalization;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest
{
    /// <summary>
    /// Setup fixture for assembly
    /// </summary>
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        private static ISessionFactory _sessionFactory;
        private static IPerson _person;

        public static ISessionFactory SessionFactory
        {
            get { return _sessionFactory; }
        }

        public static IPerson Person
        {
            get { return _person; }
        }

        /// <summary>
        /// Runs before any test.
        /// </summary>
        [SetUp]
        public void RunBeforeAnyTest()
        {
            var uowFactory = DataSourceHelper.CreateDataSource(null, null);
            _sessionFactory = (ISessionFactory)typeof(NHibernateUnitOfWorkFactory)
                .GetField("_factory", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(uowFactory.Application);
            createTestPerson();
        }

        private static void createTestPerson()
        {
            var personCreator = new PersonCreator(SessionFactory);
            var person = personCreator.Create("name", "name", "name", "name", new CultureInfo(1033), (TimeZoneInfo.Local));

        	personCreator.Save(person);
        	_person = person;
        }

        [TearDown]
        public void RunAfterTestSuite()
        {
            StateHolderProxyHelper.ClearStateHolder();
        }
    }
}

