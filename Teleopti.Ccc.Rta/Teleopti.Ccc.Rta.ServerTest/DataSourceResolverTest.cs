using System;
using System.Data;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
    [TestFixture]
    public class DataSourceResolverTest
    {
        private IDataSourceResolver target;
        private MockRepository mocks;
        private ILog loggingSvc;
        private IDatabaseConnectionFactory databaseConnectionFactory;
        private string connectionString;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            loggingSvc = mocks.StrictMock<ILog>();
            databaseConnectionFactory = mocks.StrictMock<IDatabaseConnectionFactory>();
            connectionString = "connection";
        }

        [Test]
        public void VerifyCanCreateInstance()
        {
            target = new DataSourceResolver(databaseConnectionFactory, connectionString);
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyOnlyLoadsDataOnce()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();
            IDataReader dataReader = mocks.StrictMock<IDataReader>();
            connection.Dispose();
            
            Expect.Call(databaseConnectionFactory.CreateConnection(connectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RTA.rta_load_datasources";
            connection.Open();
            Expect.Call(command.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
            Expect.Call(dataReader.Read()).Return(true);
            Expect.Call(dataReader.GetOrdinal("datasource_id")).Return(0);
            Expect.Call(dataReader.GetInt16(0)).Return(2);
            Expect.Call(dataReader["source_id"]).Return("ACD2");
            Expect.Call(dataReader.Read()).Return(false);
            dataReader.Close();

            mocks.ReplayAll();
            target = new DataSourceResolverForTest(databaseConnectionFactory, connectionString, loggingSvc);

            int resolvedId;
            Assert.IsTrue(target.TryResolveId("ACD2", out resolvedId));
            Assert.AreEqual(2,resolvedId);
            Assert.IsTrue(target.TryResolveId("ACD2", out resolvedId)); //This time all the stuff should have been loaded already, thus only one expectation for all of the above
            Assert.AreEqual(2, resolvedId);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifySkipDataSourceWithNoSourceId()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();
            IDataReader dataReader = mocks.StrictMock<IDataReader>();
            connection.Dispose();

            Expect.Call(databaseConnectionFactory.CreateConnection(connectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RTA.rta_load_datasources";
            connection.Open();
            Expect.Call(command.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
            Expect.Call(dataReader.Read()).Return(true);
            Expect.Call(dataReader["source_id"]).Return(DBNull.Value);
            Expect.Call(dataReader.GetOrdinal("datasource_id")).Return(0);
            Expect.Call(dataReader.GetInt16(0)).Return(2);
            Expect.Call(dataReader.Read()).Return(false);
            dataReader.Close();
            loggingSvc.WarnFormat("", 2);
            LastCall.IgnoreArguments();

            mocks.ReplayAll();
            target = new DataSourceResolverForTest(databaseConnectionFactory, connectionString, loggingSvc);

            int resolvedId;
            Assert.IsFalse(target.TryResolveId("ACD2", out resolvedId));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifySkipSourceThatAlreadyExistsWithInfoMessage()
        {
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();
            IDataReader dataReader = mocks.StrictMock<IDataReader>();
            connection.Dispose();

            Expect.Call(databaseConnectionFactory.CreateConnection(connectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RTA.rta_load_datasources";
            connection.Open();
            Expect.Call(command.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
            Expect.Call(dataReader.Read()).Return(true);
            Expect.Call(dataReader["source_id"]).Return("ACD2");
            Expect.Call(dataReader.GetOrdinal("datasource_id")).Return(0);
            Expect.Call(dataReader.GetInt16(0)).Return(2);
            Expect.Call(dataReader.Read()).Return(true);
            Expect.Call(dataReader["source_id"]).Return("ACD2");
            Expect.Call(dataReader.GetOrdinal("datasource_id")).Return(0);
            Expect.Call(dataReader.GetInt16(0)).Return(3);
            Expect.Call(dataReader.Read()).Return(false);
            dataReader.Close();
            loggingSvc.WarnFormat("", "ACD2");
            LastCall.IgnoreArguments();

            mocks.ReplayAll();
            target = new DataSourceResolverForTest(databaseConnectionFactory, connectionString, loggingSvc);

            int resolvedId;
            Assert.IsTrue(target.TryResolveId("ACD2", out resolvedId));
            Assert.AreEqual(2,resolvedId);
            mocks.VerifyAll();
        }
    }

    public class DataSourceResolverForTest : DataSourceResolver
    {
        public DataSourceResolverForTest(IDatabaseConnectionFactory databaseConnectionFactory, string connectionString, ILog loggingSvc)
            : base(databaseConnectionFactory, connectionString, loggingSvc)
        {
        }
    }
}
