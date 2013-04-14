﻿using System;
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
        private IDataSourceResolver _target;
        private MockRepository _mocks;
        private ILog _loggingSvc;
        private IDatabaseConnectionFactory _databaseConnectionFactory;
        private string _connectionString;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _loggingSvc = _mocks.DynamicMock<ILog>();
            _databaseConnectionFactory = _mocks.StrictMock<IDatabaseConnectionFactory>();
            _connectionString = "connection";
        }

        [Test]
        public void VerifyCanCreateInstance()
        {
            _target = new DataSourceResolver(_databaseConnectionFactory, _connectionString);
            Assert.IsNotNull(_target);
        }
		
        [Test]
        public void ShouldVerifyWholeMethod()
        {
            var connection = _mocks.StrictMock<IDbConnection>();
            var command = _mocks.StrictMock<IDbCommand>();
            var dataReader = _mocks.StrictMock<IDataReader>();
            connection.Dispose();

        	_databaseConnectionFactory.Expect(d => d.CreateConnection(_connectionString)).Return(connection);
            connection.Expect(c => c.CreateCommand()).Return(command);
			command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RTA.rta_load_datasources";
            connection.Open();
        	command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
			dataReader.Expect(d => d.Read()).Return(true).Repeat.Times(3);
			dataReader.Expect(d => d["source_id"]).Return(DBNull.Value);
			_loggingSvc.Expect(l => l.WarnFormat("", 1)).IgnoreArguments();
			dataReader.Expect(d => d["source_id"]).Return("ACD2").Repeat.Twice();
			dataReader.Expect(d => d.GetOrdinal("datasource_id")).Return(0).Repeat.Times(3);
			dataReader.Expect(d => d.GetInt16(0)).Return(2).Repeat.Twice();
			dataReader.Expect(d => d.GetInt16(0)).Return(3);
			dataReader.Expect(d => d.Read()).Return(false);
            dataReader.Close();

            _mocks.ReplayAll();
            _target = new DataSourceResolverForTest(_databaseConnectionFactory, _connectionString, _loggingSvc);

            int resolvedId;
            Assert.IsTrue(_target.TryResolveId("ACD2", out resolvedId));
            Assert.AreEqual(2,resolvedId);
            Assert.IsTrue(_target.TryResolveId("ACD2", out resolvedId)); //This time all the stuff should have been loaded already, thus only one expectation for all of the above
            Assert.AreEqual(2, resolvedId);
            _mocks.VerifyAll();
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
