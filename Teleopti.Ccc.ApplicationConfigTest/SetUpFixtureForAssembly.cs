using System;
using System.Globalization;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
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
	    public static ISessionFactory SessionFactory { get; private set; }
	    public static IPerson Person { get; private set; }

	    /// <summary>
        /// Runs before any test.
        /// </summary>
        [SetUp]
        public void RunBeforeAnyTest()
        {
            var applicationDb = DataSourceHelper.CreateDatabasesAndDataSource(null, null).Application;
            SessionFactory = ((NHibernateUnitOfWorkFactory)applicationDb).SessionFactory;
            createTestPerson();
        }

        private static void createTestPerson()
        {
            var personCreator = new PersonCreator(SessionFactory);
            var person = personCreator.Create("name", "name",  new CultureInfo(1033), (TimeZoneInfo.Local));

        	personCreator.Save(person);
        	Person = person;
        }

        [TearDown]
        public void RunAfterTestSuite()
        {
            StateHolderProxyHelper.ClearStateHolder();
        }
    }
}

