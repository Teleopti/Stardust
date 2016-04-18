using System;
using System.Security.Principal;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    /// <summary>
    /// Tests for DatasourceException
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class DataSourceExceptionTest : ExceptionTest<ExplicitlySetGenericPrincipalDataSourceException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected override ExplicitlySetGenericPrincipalDataSourceException CreateTestInstance(string message, Exception innerException)
        {
            return new ExplicitlySetGenericPrincipalDataSourceException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override ExplicitlySetGenericPrincipalDataSourceException CreateTestInstance(string message)
        {
            return new ExplicitlySetGenericPrincipalDataSourceException(message);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void ExceptionShouldReturnDataSourceNameFromCurrentTeleoptiIdentity()
        {
            ExplicitlySetGenericPrincipalDataSourceException exception = new ExplicitlySetGenericPrincipalDataSourceException("Test");
            string datasourceName = exception.DataSource;
            Assert.AreEqual("[not set]", datasourceName);

        }

        [Test]
        public void ExceptionShouldReturnDataSourceNameFromTheCurrentTeleoptiIdentity()
        {
            var loggedOnPerson = PersonFactory.CreatePerson("UserThatClaenUpDataSource", string.Empty);
            var businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var identity = new TeleoptiIdentity("test user", dataSource, businessUnit, WindowsIdentity.GetCurrent(), null);
            var principalForTest = new TeleoptiPrincipal(identity, loggedOnPerson);

        	dataSource.Stub(x => x.DataSourceName).Return("DataSource");

            var exception = new ExplicitlySetGenericPrincipalDataSourceException(principalForTest);
            var datasourceName = exception.DataSource;
            Assert.AreEqual("DataSource", datasourceName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void ExceptionShouldReturnSpecialDataSourceNameIfNoUserIsLoggedOn()
        {
            // if no user is logged on, then the Current thread's principal is a generic principal instead of a teleopti principal
            ExplicitlySetGenericPrincipalDataSourceException exception = new ExplicitlySetGenericPrincipalDataSourceException(new GenericPrincipal(new GenericIdentity("Test"), new string[0]));
            string datasourceName = exception.DataSource;
            Assert.AreEqual("[unknown datasource]", datasourceName);
        }
    }
}