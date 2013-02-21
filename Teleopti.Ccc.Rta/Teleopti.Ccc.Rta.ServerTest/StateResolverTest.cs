using System;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using log4net;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class StateResolverTest
	{
		private IStateResolver _target;
		private MockRepository _mock;
		private IDatabaseConnectionFactory _databaseConnectionFactory;
		private ILog _loggingSvc;
		private string _connectionString;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_databaseConnectionFactory = _mock.StrictMock<IDatabaseConnectionFactory>();
			_connectionString = "connection";
			_loggingSvc = _mock.DynamicMock<ILog>();
			_target = new StateResolverForTest(_databaseConnectionFactory, _connectionString, _loggingSvc);
		}

		[Test]
		public void ShouldHaveWorkingConstructor()
		{
			var newTarget = new StateResolver(_databaseConnectionFactory, _connectionString);
			Assert.That(newTarget, Is.Not.Null);
		}

		[Test]
		public void ShouldReturnValidDataAndInitializeDictionary()
		{
			var personId = Guid.NewGuid();
			var connection = _mock.StrictMock<IDbConnection>();
			var command = _mock.StrictMock<IDbCommand>();
			var dataReader = _mock.StrictMock<IDataReader>();
			connection.Dispose();

			_databaseConnectionFactory.Expect(d => d.CreateConnection(_connectionString)).Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "RTA.rta_load_actual_agent_statecode";
			connection.Open();

			command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
			dataReader.Expect(d => d.Read()).Return(true);
			dataReader.Expect(d => d.GetOrdinal("StateCode")).Return(0);
			dataReader.Expect(d => d.GetString(0)).Return("AUX3");
			dataReader.Expect(d => d.GetOrdinal("PersonId")).Return(1);
			dataReader.Expect(d => d.GetGuid(1)).Return(personId);
			dataReader.Expect(d => d.Read()).Return(false);
			dataReader.Expect(d => d.Close());
			_mock.ReplayAll();

			Assert.That(_target.HaveStateCodeChanged(personId, "AUX2"), Is.True);
			Assert.That(_target.HaveStateCodeChanged(Guid.NewGuid(), "OFF"), Is.True);
			Assert.That(_target.HaveStateCodeChanged(personId, "OFF"), Is.True);
			Assert.That(_target.HaveStateCodeChanged(personId, "OFF"), Is.False);
			_mock.VerifyAll();
		}
	}

	public class StateResolverForTest : StateResolver
	{
		public StateResolverForTest(IDatabaseConnectionFactory databaseConnectionFactory,
		                            string connectionString, ILog loggingSvc)
			: base(databaseConnectionFactory, connectionString, loggingSvc)
		{
		}
	}
}
