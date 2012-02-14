using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Rta.ServerTest
{
    [TestFixture]
    [Category("LongRunning")]
    public class RtaDataHandlerTest
    {
        private MockRepository mocks;
        private ILog loggingSvc;
        private IMessageSender messageSender;
        private IDatabaseConnectionFactory databaseConnectionFactory;
        private const string ConnectionString = "connection";
        private RtaDataHandlerForTest target;
        private IDataSourceResolver dataSourceResolver;
        private IPersonResolver personResolver;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            loggingSvc = mocks.DynamicMock<ILog>();
            messageSender = mocks.StrictMock<IMessageSender>();
            databaseConnectionFactory = mocks.StrictMock<IDatabaseConnectionFactory>();
            dataSourceResolver = mocks.StrictMock<IDataSourceResolver>();
            personResolver = mocks.DynamicMock<IPersonResolver>();
        }

        [Test, ExpectedException(typeof(BrokerNotInstantiatedException))]
        public void VerifyCreateInstanceUsingEmptyConstructorFailsBecauseNoConfigurationAvailable()
        {
            new RtaDataHandler();
        }

        [Test]
        public void VerifyErrorWhenMessageBrokerNotInstantiated()
        {
            loggingSvc.Error("", new BrokerNotInstantiatedException());
            LastCall.IgnoreArguments();
            messageSender.InstantiateBrokerService();
            LastCall.Throw(new BrokerNotInstantiatedException());
            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyProperties()
        {
            messageSender.InstantiateBrokerService();
            Expect.Call(messageSender.IsAlive).Return(true);
            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
            Assert.IsTrue(target.IsAlive);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyWarningWithNoConnectionString()
        {
            DateTime timestamp = DateTime.UtcNow;
            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId("1",
                                                        out dataSourceAfterResolve)).Return(true).OutRef(1);
            IEnumerable<Guid> personListAfterResolve;
            Expect.Call(personResolver.TryResolveId(1, "002", out personListAfterResolve)).Return(true).
                OutRef(new[] { Guid.NewGuid() });

            messageSender.InstantiateBrokerService();
            messageSender.SendRtaData(Guid.Empty, null);
            LastCall.IgnoreArguments();
            Expect.Call(messageSender.IsAlive).Return(true);
            loggingSvc.Warn("No connection information available in configuration file.");

            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, string.Empty, databaseConnectionFactory, dataSourceResolver, personResolver);
            target.ProcessRtaData("002", "AUX2", TimeSpan.FromSeconds(45), timestamp, Guid.NewGuid(), "1",
                                  SqlDateTime.MinValue.Value, false);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifySnapshotItemInsertedWithConnectionString()
        {
            TimeSpan timeInState = TimeSpan.FromSeconds(45);
            DateTime timestamp = DateTime.UtcNow;
            Guid platformTypeId = Guid.NewGuid();
            DateTime batchId = DateTime.UtcNow;
            const int dataSourceId = 1;
            const bool isSnapshot = true;

            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();
            IDataParameterCollection parameterCollection = mocks.StrictMock<IDataParameterCollection>();
            IDbDataParameter dataParameter = mocks.StrictMock<IDbDataParameter>();
            connection.Dispose();
            messageSender.InstantiateBrokerService();
            Expect.Call(messageSender.IsAlive).Return(false);
            Expect.Call(databaseConnectionFactory.CreateConnection(ConnectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);
            Expect.Call(command.Parameters).Return(parameterCollection).Repeat.AtLeastOnce();
            Expect.Call(command.CreateParameter()).Return(dataParameter).Repeat.AtLeastOnce();
            Expect.Call(parameterCollection.Add(dataParameter)).Return(1).IgnoreArguments().Repeat.AtLeastOnce();
            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId(dataSourceId.ToString(CultureInfo.InvariantCulture),
                                                        out dataSourceAfterResolve)).Return(true).OutRef(dataSourceId);
            IEnumerable<Guid> personListAfterResolve;
            Expect.Call(personResolver.TryResolveId(dataSourceId, "002", out personListAfterResolve)).Return(true).
                OutRef(new[] { Guid.NewGuid() });

            //This is the important stuff! Checking parameters and values
            dataParameter.ParameterName = "@LogOn";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = "002";
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@StateCode";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = "AUX2";
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@TimeInState";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timeInState.Ticks;
            dataParameter.DbType = DbType.Int64;

            dataParameter.ParameterName = "@Timestamp";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timestamp;
            dataParameter.DbType = DbType.DateTime;

            dataParameter.ParameterName = "@PlatformTypeId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = platformTypeId;
            dataParameter.DbType = DbType.Guid;

            dataParameter.ParameterName = "@DataSourceId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = dataSourceId;
            dataParameter.DbType = DbType.Int32;

            dataParameter.ParameterName = "@BatchId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = batchId;
            dataParameter.DbType = DbType.DateTime;

            dataParameter.ParameterName = "@IsSnapshot";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = isSnapshot;
            dataParameter.DbType = DbType.Boolean;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RTA.rta_insert_agentstate";
            connection.Open();
            Expect.Call(command.ExecuteNonQuery()).Return(1);

            Expect.Call(connection.Database).Return("db1");
            LastCall.IgnoreArguments();
            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
			var waitHandle = target.ProcessRtaData("002", "AUX2", timeInState, timestamp, platformTypeId,
                                  dataSourceId.ToString(CultureInfo.InvariantCulture), batchId, isSnapshot);
			waitHandle.WaitOne();

			mocks.VerifyAll();
		}

        [Test]
        public void VerifyItemInsertedWithConnectionString()
        {
            TimeSpan timeInState = TimeSpan.FromSeconds(45);
            DateTime timestamp = DateTime.UtcNow;
            Guid platformTypeId = Guid.NewGuid();
            DateTime batchId = SqlDateTime.MinValue.Value;
            const int dataSourceId = 1;
            const bool isSnapshot = false;

            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();
            IDataParameterCollection parameterCollection = mocks.StrictMock<IDataParameterCollection>();
            IDbDataParameter dataParameter = mocks.StrictMock<IDbDataParameter>();
            connection.Dispose();
            messageSender.InstantiateBrokerService();
            Expect.Call(messageSender.IsAlive).Return(false);
            Expect.Call(databaseConnectionFactory.CreateConnection(ConnectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);
            Expect.Call(command.Parameters).Return(parameterCollection).Repeat.AtLeastOnce();
            Expect.Call(command.CreateParameter()).Return(dataParameter).Repeat.AtLeastOnce();
            Expect.Call(parameterCollection.Add(dataParameter)).Return(1).IgnoreArguments().Repeat.AtLeastOnce();
            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId(dataSourceId.ToString(CultureInfo.InvariantCulture),
                                                        out dataSourceAfterResolve)).Return(true).OutRef(dataSourceId);
            IEnumerable<Guid> personListAfterResolve;
            Expect.Call(personResolver.TryResolveId(dataSourceId, "002", out personListAfterResolve)).Return(true).
                OutRef(new[] {Guid.NewGuid()});

            //This is the important stuff! Checking parameters and values
            dataParameter.ParameterName = "@LogOn";
            dataParameter.Value = "002";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@StateCode";
            dataParameter.Value = "AUX2";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@TimeInState";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timeInState.Ticks;
            dataParameter.DbType = DbType.Int64;

            dataParameter.ParameterName = "@Timestamp";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timestamp;
            dataParameter.DbType = DbType.DateTime;

            dataParameter.ParameterName = "@PlatformTypeId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = platformTypeId;
            dataParameter.DbType = DbType.Guid;

            dataParameter.ParameterName = "@DataSourceId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = dataSourceId;
            dataParameter.DbType = DbType.Int32;

            dataParameter.ParameterName = "@BatchId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = batchId;
            dataParameter.DbType = DbType.DateTime;

            dataParameter.ParameterName = "@IsSnapshot";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = isSnapshot;
            dataParameter.DbType = DbType.Boolean;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RTA.rta_insert_agentstate";
            connection.Open();
            Expect.Call(command.ExecuteNonQuery()).Return(1);

            Expect.Call(connection.Database).Return("db1");
            LastCall.IgnoreArguments();
            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
			var waitHandle = target.ProcessRtaData("002", "AUX2", timeInState, timestamp, platformTypeId, dataSourceId.ToString(CultureInfo.InvariantCulture), batchId,
                                  isSnapshot);
			waitHandle.WaitOne();

			mocks.VerifyAll();
		}

        [Test]
        public void VerifySqlExceptionIsHandled()
        {
            messageSender.InstantiateBrokerService();
            
            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId("1",
                                                        out dataSourceAfterResolve)).Return(true).OutRef(1);
            IEnumerable<Guid> personListAfterResolve;
            Expect.Call(personResolver.TryResolveId(1, "002", out personListAfterResolve)).Return(true).
                OutRef(new[] { Guid.NewGuid() });

            DbException exception = mocks.StrictMock<DbException>();
            Expect.Call(messageSender.IsAlive).Return(false);
            Expect.Call(databaseConnectionFactory.CreateConnection(ConnectionString)).Throw(exception);
            loggingSvc.Error("",exception);
            LastCall.IgnoreArguments();
            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
			var waitHandle = target.ProcessRtaData("002", "AUX2", TimeSpan.FromSeconds(45), DateTime.UtcNow, Guid.NewGuid(), "1",
                                  SqlDateTime.MinValue.Value, false);
			waitHandle.WaitOne();

			mocks.VerifyAll();
		}

        [Test]
        public void VerifySqlDateTimeOverflowAsBatchIdIsHandled()
        {
            TimeSpan timeInState = TimeSpan.FromSeconds(45);
            DateTime timestamp = DateTime.UtcNow;
            Guid platformTypeId = Guid.NewGuid();
            DateTime batchId = DateTime.MinValue;
            const int dataSourceId = 1;
            const bool isSnapshot = false;

            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();
            IDataParameterCollection parameterCollection = mocks.StrictMock<IDataParameterCollection>();
            IDbDataParameter dataParameter = mocks.StrictMock<IDbDataParameter>();
            connection.Dispose();
            messageSender.InstantiateBrokerService();
            Expect.Call(messageSender.IsAlive).Return(false);
            Expect.Call(databaseConnectionFactory.CreateConnection(ConnectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);
            Expect.Call(command.Parameters).Return(parameterCollection).Repeat.AtLeastOnce();
            Expect.Call(command.CreateParameter()).Return(dataParameter).Repeat.AtLeastOnce();
            Expect.Call(parameterCollection.Add(dataParameter)).Return(1).IgnoreArguments().Repeat.AtLeastOnce();
            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId(dataSourceId.ToString(CultureInfo.InvariantCulture),
                                                        out dataSourceAfterResolve)).Return(true).OutRef(dataSourceId);
            
            IEnumerable<Guid> personListAfterResolve;
            Expect.Call(personResolver.TryResolveId(dataSourceId, "002", out personListAfterResolve)).Return(true).
                OutRef(new[] { Guid.NewGuid() });

            //This is the important stuff! Checking parameters and values
            dataParameter.ParameterName = "@LogOn";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = "002";
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@StateCode";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = "AUX2";
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@TimeInState";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timeInState.Ticks;
            dataParameter.DbType = DbType.Int64;

            dataParameter.ParameterName = "@Timestamp";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timestamp;
            dataParameter.DbType = DbType.DateTime;

            dataParameter.ParameterName = "@PlatformTypeId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = platformTypeId;
            dataParameter.DbType = DbType.Guid;

            dataParameter.ParameterName = "@DataSourceId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = dataSourceId;
            dataParameter.DbType = DbType.Int32;

            dataParameter.ParameterName = "@BatchId";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timestamp;
            dataParameter.DbType = DbType.DateTime;

            dataParameter.ParameterName = "@IsSnapshot";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = isSnapshot;
            dataParameter.DbType = DbType.Boolean;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RTA.rta_insert_agentstate";
            connection.Open();
            Expect.Call(command.ExecuteNonQuery()).Return(1);

            Expect.Call(connection.Database).Return("db1");
            LastCall.IgnoreArguments();
            loggingSvc.WarnFormat("", "");
            LastCall.IgnoreArguments();

            mocks.ReplayAll();

            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
            var waitHandle = target.ProcessRtaData("002", "AUX2", timeInState, timestamp, platformTypeId, dataSourceId.ToString(CultureInfo.InvariantCulture), batchId,
                                  isSnapshot);
        	waitHandle.WaitOne();

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyErrorToLogWhenNoDataSourceCanBeResolved()
        {
            messageSender.InstantiateBrokerService();

            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId("1",
                                                        out dataSourceAfterResolve)).Return(false).OutRef(0);

            loggingSvc.ErrorFormat("", "1");
            LastCall.IgnoreArguments();
            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
			var waitHandle = target.ProcessRtaData("002", "AUX2", TimeSpan.FromSeconds(45), DateTime.UtcNow, Guid.NewGuid(), "1",
                                  SqlDateTime.MinValue.Value, false);
			waitHandle.WaitOne();

			mocks.VerifyAll();
		}

        [Test]
        public void VerifyWarningToLogWhenNoPersonCanBeResolved()
        {
            messageSender.InstantiateBrokerService();

            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId("1",
                                                        out dataSourceAfterResolve)).Return(true).OutRef(1);

            IEnumerable<Guid> personListAfterResolve;
            Expect.Call(personResolver.TryResolveId(1, "002", out personListAfterResolve)).Return(false).
                OutRef(new Guid[]{});

            loggingSvc.WarnFormat("", "1", "002");
            LastCall.IgnoreArguments();

            IDbConnection connection = mocks.DynamicMock<IDbConnection>();
            IDbCommand command = mocks.DynamicMock<IDbCommand>();
            IDataParameterCollection parameterCollection = mocks.DynamicMock<IDataParameterCollection>();
            IDbDataParameter dataParameter = mocks.DynamicMock<IDbDataParameter>();
            connection.Dispose();

            Expect.Call(messageSender.IsAlive).Return(true);
            Expect.Call(databaseConnectionFactory.CreateConnection(ConnectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);
            Expect.Call(command.Parameters).Return(parameterCollection).Repeat.AtLeastOnce();
            Expect.Call(command.CreateParameter()).Return(dataParameter).Repeat.AtLeastOnce();
            Expect.Call(parameterCollection.Add(dataParameter)).Return(1).IgnoreArguments().Repeat.AtLeastOnce();

            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
            var waitHandle = target.ProcessRtaData("002", "AUX2", TimeSpan.FromSeconds(45), DateTime.UtcNow, Guid.NewGuid(), "1",
                                  SqlDateTime.MinValue.Value, false);
            waitHandle.WaitOne();

            mocks.VerifyAll();
        }

        [Test]
        public void VerifySqlDateTimeOverflowAsTimestampIsHandled()
        {
            TimeSpan timeInState = TimeSpan.FromSeconds(45);
            DateTime timestamp = DateTime.MinValue;
            Guid platformTypeId = Guid.NewGuid();
            DateTime batchId = DateTime.MinValue;
            const int dataSourceId = 1;
            const bool isSnapshot = false;

            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            IDbCommand command = mocks.StrictMock<IDbCommand>();
            IDataParameterCollection parameterCollection = mocks.StrictMock<IDataParameterCollection>();
            IDbDataParameter dataParameter = mocks.StrictMock<IDbDataParameter>();
            connection.Dispose();
            messageSender.InstantiateBrokerService();
            Expect.Call(messageSender.IsAlive).Return(false);
            Expect.Call(databaseConnectionFactory.CreateConnection(ConnectionString)).Return(connection);
            Expect.Call(connection.CreateCommand()).Return(command);
            Expect.Call(command.Parameters).Return(parameterCollection).Repeat.AtLeastOnce();
            Expect.Call(command.CreateParameter()).Return(dataParameter).Repeat.AtLeastOnce();
            Expect.Call(parameterCollection.Add(dataParameter)).Return(1).IgnoreArguments().Repeat.AtLeastOnce();

            int dataSourceAfterResolve;
            Expect.Call(dataSourceResolver.TryResolveId(dataSourceId.ToString(CultureInfo.InvariantCulture),
                                                        out dataSourceAfterResolve)).Return(true).OutRef(dataSourceId);

            IEnumerable<Guid> personListAfterResolve;
            Expect.Call(personResolver.TryResolveId(dataSourceId, "002", out personListAfterResolve)).Return(true).
                OutRef(new[] { Guid.NewGuid() });

            //This is the important stuff! Checking parameters and values
            dataParameter.ParameterName = "@LogOn";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = "002";
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@StateCode";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = "AUX2";
            dataParameter.DbType = DbType.String;
            dataParameter.Size = 50;

            dataParameter.ParameterName = "@TimeInState";
            dataParameter.Direction = ParameterDirection.Input;
            dataParameter.Value = timeInState.Ticks;
            dataParameter.DbType = DbType.Int64;

            loggingSvc.ErrorFormat("", "");
            LastCall.IgnoreArguments();
            mocks.ReplayAll();
            target = new RtaDataHandlerForTest(loggingSvc, messageSender, ConnectionString, databaseConnectionFactory, dataSourceResolver, personResolver);
			var waitHandle = target.ProcessRtaData("002", "AUX2", timeInState, timestamp, platformTypeId, dataSourceId.ToString(CultureInfo.InvariantCulture), batchId,
                                  isSnapshot);
			waitHandle.WaitOne();

			mocks.VerifyAll();
		}

        private class RtaDataHandlerForTest : RtaDataHandler
        {
            public RtaDataHandlerForTest(ILog loggingSvc, IMessageSender messageSender, string connectionStringDataStore, IDatabaseConnectionFactory databaseConnectionFactory, IDataSourceResolver dataSourceResolver, IPersonResolver personResolver)
                : base(loggingSvc, messageSender, connectionStringDataStore,  databaseConnectionFactory, dataSourceResolver, personResolver)
            {
            }
        }
    }
}
