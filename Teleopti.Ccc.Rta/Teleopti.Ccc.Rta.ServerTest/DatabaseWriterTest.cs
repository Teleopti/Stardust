using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.ServerTest
{
    [TestFixture]
    public class DatabaseWriterTest
    {
        private DatabaseWriter _target;

        private MockRepository _mock;
        private IDatabaseConnectionFactory _connectionFactory;
        private IDatabaseConnectionStringHandler _stringHandler;

        private IDbConnection _connection;
        private IDbCommand _command;
        private IDataReader _reader;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _connectionFactory = _mock.StrictMock<IDatabaseConnectionFactory>();
            _stringHandler = _mock.StrictMock<IDatabaseConnectionStringHandler>();
            _target = new DatabaseWriter(_connectionFactory, _stringHandler);

            _connection = _mock.StrictMock<IDbConnection>();
            _command = _mock.StrictMock<IDbCommand>();
            _reader = _mock.StrictMock<IDataReader>();
        }

        [Test]
        public void VerifyAddOrUpdate()
        {
            var agentState = new ActualAgentState();
            var batchAgentState = new ActualAgentState { BatchId = new DateTime() };
            var dataParameters = _mock.StrictMock<IDataParameterCollection>();

            _stringHandler.Expect(sh => sh.DataStoreConnectionString()).Return("connectionString");
            _connectionFactory.Expect(cf => cf.CreateConnection("connectionString")).Return(_connection);
            _connection.Expect(c => c.CreateCommand()).Return(_command).Repeat.Twice();

            _command.Expect(c => c.CommandType = CommandType.StoredProcedure).Repeat.Twice();
            _command.Expect(c => c.CommandText = "[RTA].[rta_addorupdate_actualagentstate]").Repeat.Twice();

            _command.Expect(c => c.Parameters).Return(dataParameters).Repeat.Any();
            dataParameters.Expect(d => d.Add(new SqlParameter())).IgnoreArguments().Return(0).Repeat.Any();

            _connection.Expect(c => c.Open());
            _command.Expect(c => c.ExecuteNonQuery()).Return(1).Repeat.Twice();
            _connection.Expect(c => c.Dispose());
            _mock.ReplayAll();

            _target.AddOrUpdate(new List<IActualAgentState> { agentState, batchAgentState });
            _mock.VerifyAll();
        }

        [Test]
        public void AddOrUpdate_NoStates()
        {
            _mock.ReplayAll();
            _target.AddOrUpdate(new List<IActualAgentState>());
            _connectionFactory.AssertWasNotCalled(c => c.CreateConnection(""), c => c.IgnoreArguments());
        }

        [Test]
        public void AddNewRtaState_AddNewState()
        {
            Guid stateGroupId = Guid.NewGuid();

            _stringHandler.Expect(s => s.AppConnectionString()).Return("connection");
            _connectionFactory.Expect(c => c.CreateConnection("connection")).Return(_connection);
            _connection.Expect(c => c.CreateCommand()).Return(_command);

            _command.Expect(c => c.CommandType = CommandType.Text);
            _command.Expect(c => c.CommandText = "").IgnoreArguments();
            _connection.Expect(c => c.Open());
            _command.Expect(c => c.ExecuteReader()).Return(_reader);

            _reader.Expect(r => r.Read()).Return(true);
            _reader.Expect(r => r.GetOrdinal("Id")).Return(0);
            _reader.Expect(r => r.GetGuid(0)).Return(stateGroupId);
            _reader.Expect(r => r.GetOrdinal("Name")).Return(1);
            _reader.Expect(r => r.GetString(1)).Return("stateName");

            _reader.Expect(r => r.Read()).Return(false);
            _reader.Expect(r => r.Close());

            _connection.Expect(c => c.CreateCommand()).Return(_command);
            _command.Expect(c => c.CommandText = "").IgnoreArguments();
            _command.Expect(c => c.ExecuteNonQuery()).Return(1);
            _connection.Expect(c => c.Dispose());

            _mock.ReplayAll();

            var businessUnit = Guid.NewGuid();
            var result = _target.AddAndGetNewRtaState("stateCode", Guid.NewGuid(), businessUnit);
            result.StateGroupId.Should().Be.EqualTo(stateGroupId);
            result.StateGroupName.Should().Be.EqualTo("stateName");
            _mock.VerifyAll();
        }

        [Test]
        public void AddNewRtaState_AddNewState_ForCorrectBusinessUnit()
        {
            Guid stateGroupId = Guid.NewGuid();

            _stringHandler.Expect(s => s.AppConnectionString()).Return("connection");
            _connectionFactory.Expect(c => c.CreateConnection("connection")).Return(_connection);
            _connection.Expect(c => c.CreateCommand()).Return(_command);

            _command.Expect(c => c.CommandType = CommandType.Text);
            _command.Expect(c => c.CommandText = "").IgnoreArguments();
            _connection.Expect(c => c.Open());
            _command.Expect(c => c.ExecuteReader()).Return(_reader);

            _reader.Expect(r => r.Read()).Return(true);
            _reader.Expect(r => r.GetOrdinal("Id")).Return(0);
            _reader.Expect(r => r.GetGuid(0)).Return(stateGroupId);
            _reader.Expect(r => r.GetOrdinal("Name")).Return(1);
            _reader.Expect(r => r.GetString(1)).Return("stateName");

            _reader.Expect(r => r.Read()).Return(false);
            _reader.Expect(r => r.Close());

            _connection.Expect(c => c.CreateCommand()).Return(_command);
            _command.Expect(c => c.CommandText = "").IgnoreArguments();
            _command.Expect(c => c.ExecuteNonQuery()).Return(1);
            _connection.Expect(c => c.Dispose());

            _mock.ReplayAll();

            var businessUnit = Guid.NewGuid();
            var result = _target.AddAndGetNewRtaState("stateCode", Guid.NewGuid(), businessUnit);
            result.BusinessUnitId.Should().Be.EqualTo(businessUnit);
            result.StateGroupId.Should().Be.EqualTo(stateGroupId);
            result.StateGroupName.Should().Be.EqualTo("stateName");
            _mock.VerifyAll();
        }
    }
}