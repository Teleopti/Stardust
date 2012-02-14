using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class AvailableDataSourcesProviderTest
    {
        private IAvailableDataSourcesProvider target;
        private MockRepository mocks;
        private IApplicationData applicationData;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            applicationData = mocks.StrictMock<IApplicationData>();
            target = new AvailableDataSourcesProvider(applicationData);
        }

        [Test]
        public void VerifyCanGetAvailableDataSources()
        {
            var dataSource = mocks.StrictMock<IDataSource>();
            IList<IDataSource> dataSources = new List<IDataSource> { dataSource };
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(applicationData.RegisteredDataSourceCollection).Return(dataSources);
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var result = target.AvailableDataSources();
                Assert.AreEqual(1, result.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), Test]
        public void VerifyCanGetUnavailableDataSources()
        {
            var dataSource = mocks.StrictMock<IDataSource>();
            IList<IDataSource> dataSources = new List<IDataSource> { dataSource };
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            using (mocks.Record())
            {
                Expect.Call(applicationData.RegisteredDataSourceCollection).Return(dataSources);
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Throw(new Exception("test",CreateSqlException(4060)));
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
            }
            using (mocks.Playback())
            {
                var result = target.UnavailableDataSources();
                Assert.AreEqual(1, result.Count());
            }
        }

        private static T Construct<T>(params object[] p)
        {
            return (T)typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0].Invoke(p);
        }

        private static SqlException CreateSqlException(int errorNumber)
        {
            SqlErrorCollection collection = Construct<SqlErrorCollection>();
            SqlError error = Construct<SqlError>(errorNumber, (byte)2, (byte)3, "server name", "error message", "proc", 100);

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(collection, new object[] { error });

            var e = typeof(SqlException)
                        .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static)
                        .Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
            return e;
        }
    }
}