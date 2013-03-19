﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class ActualAgentDataHandlerTest
	{
		private ActualAgentDataHandler _target;

		private MockRepository _mock;
		private IDatabaseConnectionFactory _connectionFactory;
		private IDatabaseConnectionStringHandler _stringHandler;

		private IDbConnection _connection;
		private IDbCommand _command;
		private IDataReader _reader;
		private readonly Guid _guid = Guid.NewGuid();
		private DateTime _dateTime = DateTime.UtcNow;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_connectionFactory = _mock.StrictMock<IDatabaseConnectionFactory>();
			_stringHandler = _mock.StrictMock<IDatabaseConnectionStringHandler>();
			_target = new ActualAgentDataHandler(_connectionFactory, _stringHandler);

			_connection = _mock.StrictMock<IDbConnection>();
			_command = _mock.StrictMock<IDbCommand>();
			_reader = _mock.StrictMock<IDataReader>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
		public void VerifyGetReadModel()
		{
			_stringHandler.Expect(sh => sh.AppConnectionString()).Return("connectionString");
			_connectionFactory.Expect(cf => cf.CreateConnection("connectionString")).Return(_connection);
			_connection.Expect(c => c.CreateCommand()).Return(_command);

			_command.CommandType = CommandType.Text;
			_command.Expect(c => c.CommandText).SetPropertyAndIgnoreArgument();
			_connection.Open();
			_command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(_reader);

			_reader.Expect(r => r.Read()).Return(true).Repeat.Twice();
			_reader.Expect(r => r.GetOrdinal("PayloadId")).Return(0).Repeat.Twice();
			_reader.Expect(r => r.GetGuid(0)).Return(_guid).Repeat.Twice();
			_reader.Expect(r => r.GetOrdinal("StartDateTime")).Return(1).Repeat.Twice();
			_reader.Expect(r => r.GetDateTime(1)).Return(_dateTime).Repeat.Twice();
			_reader.Expect(r => r.GetOrdinal("EndDateTime")).Return(2).Repeat.Twice();
			_reader.Expect(r => r.GetDateTime(2)).Return(_dateTime).Repeat.Twice();
			_reader.Expect(r => r.GetOrdinal("Name")).Return(3).Repeat.Twice();
			_reader.Expect(r => r.GetString(3)).Return("Morning").Repeat.Twice();
			_reader.Expect(r => r.GetOrdinal("ShortName")).Return(4).Repeat.Twice();
			_reader.Expect(r => r.GetString(4)).Return("Mo").Repeat.Twice();
			_reader.Expect(r => r.GetOrdinal("DisplayColor")).Return(5).Repeat.Twice();
			_reader.Expect(r => r.GetInt32(5)).Return(1234567989).Repeat.Twice();

			_reader.Expect(r => r.Read()).Return(false);
			_reader.Expect(r => r.Close());

			_connection.Dispose();
			_mock.ReplayAll();

			var result = _target.GetReadModel(_guid);
			_mock.VerifyAll();

			result.Count.Should().Be.EqualTo(2);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), Test]
		public void VerifyLoadOldState()
		{
			_stringHandler.Expect(sh => sh.DataStoreConnectionString()).Return("connectionString");
			_connectionFactory.Expect(cf => cf.CreateConnection("connectionString")).Return(_connection);
			_connection.Expect(c => c.CreateCommand()).Return(_command);

			_command.CommandType = CommandType.Text;
			_command.Expect(c => c.CommandText).SetPropertyAndIgnoreArgument();
			_connection.Open();
			_command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(_reader);

			_reader.Expect(r => r.Read()).Return(true);
			_reader.Expect(r => r.GetOrdinal("StateCode")).Return(0);
			_reader.Expect(r => r.GetString(0)).Return("myStateCode");

			_reader.Expect(r => r.GetOrdinal("PlatformTypeId")).Return(5);
			_reader.Expect(r => r.GetGuid(5)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("ScheduledId")).Return(6);
			_reader.Expect(r => r.GetGuid(6)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("AlarmId")).Return(7);
			_reader.Expect(r => r.GetGuid(7)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("ScheduledNextId")).Return(8);
			_reader.Expect(r => r.GetGuid(8)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("StateId")).Return(10);
			_reader.Expect(r => r.GetGuid(10)).Return(_guid);

			_reader.Expect(r => r.GetOrdinal("StateStart")).Return(11);
			_reader.Expect(r => r.GetDateTime(11)).Return(_dateTime);
			_reader.Expect(r => r.GetOrdinal("NextStart")).Return(12);
			_reader.Expect(r => r.GetDateTime(12)).Return(_dateTime);

			_reader.Expect(r => r.Dispose());
			_connection.Expect(c => c.Dispose());
			_mock.ReplayAll();

			var result = _target.LoadOldState(_guid);
			_mock.VerifyAll();

			Assert.IsNotNull(result);
			result.StateCode.Should().Be.EqualTo("myStateCode");
			result.PlatformTypeId.Should().Be.EqualTo(_guid);
			result.StateStart.Should().Be.EqualTo(_dateTime);
		}

		[Test]
		public void VerifyLoadOldStateWithNoData()
		{
			_stringHandler.Expect(sh => sh.DataStoreConnectionString()).Return("connectionString");
			_connectionFactory.Expect(cf => cf.CreateConnection("connectionString")).Return(_connection);
			_connection.Expect(c => c.CreateCommand()).Return(_command);

			_command.CommandType = CommandType.Text;
			_command.Expect(c => c.CommandText).SetPropertyAndIgnoreArgument();
			_connection.Open();
			_command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(_reader);
			_reader.Expect(r => r.Read()).Return(false);
			_reader.Expect(r => r.Dispose());
			_connection.Expect(c => c.Dispose());
			_mock.ReplayAll();

			var result = _target.LoadOldState(_guid);
			result.Should().Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
		public void VerifyStateGroups()
		{
			_stringHandler.Expect(sh => sh.AppConnectionString()).Return("connectionString");
			_connectionFactory.Expect(cf => cf.CreateConnection("connectionString")).Return(_connection);
			_connection.Expect(c => c.CreateCommand()).Return(_command);

			_command.CommandType = CommandType.Text;
			_command.Expect(c => c.CommandText).SetPropertyAndIgnoreArgument();
			_connection.Open();
			_command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(_reader);

			_reader.Expect(r => r.Read()).Return(true);
			_reader.Expect(r => r.GetOrdinal("StateName")).Return(0);
			_reader.Expect(r => r.GetString(0)).Return("stateName");
			_reader.Expect(r => r.GetOrdinal("StateGroupId")).Return(1);
			_reader.Expect(r => r.GetGuid(1)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("StateId")).Return(2);
			_reader.Expect(r => r.GetGuid(2)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("StateCode")).Return(3);
			_reader.Expect(r => r.GetString(3)).Return("stateCode");
			_reader.Expect(r => r.GetOrdinal("BusinessUnitId")).Return(4);
			_reader.Expect(r => r.GetGuid(4)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("PlatformTypeId")).Return(5);
			_reader.Expect(r => r.GetGuid(5)).Return(_guid);
			_reader.Expect(r => r.Read()).Return(false);
			_reader.Expect(r => r.Close());

			_connection.Expect(c => c.Dispose());
			_mock.ReplayAll();

			var result = _target.StateGroups().ToList();
			_mock.VerifyAll();

			result.Count().Should().Be.EqualTo(1);
			result[0].Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
		public void VerifyActivityAlarms()
		{
			_stringHandler.Expect(sh => sh.AppConnectionString()).Return("connectionString");
			_connectionFactory.Expect(cf => cf.CreateConnection("connectionString")).Return(_connection);
			_connection.Expect(c => c.CreateCommand()).Return(_command);

			_command.CommandType = CommandType.Text;
			_command.Expect(c => c.CommandText).SetPropertyAndIgnoreArgument();
			_connection.Open();
			_command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(_reader);

			_reader.Expect(r => r.Read()).Return(true);
			_reader.Expect(r => r.GetOrdinal("StateGroupName")).Return(0);
			_reader.Expect(r => r.GetString(0)).Return("stateGroupName");
			_reader.Expect(r => r.GetOrdinal("StateGroupId")).Return(1);
			_reader.Expect(r => r.GetGuid(1)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("AlarmTypeId")).Return(2);
			_reader.Expect(r => r.GetGuid(2)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("StaffingEffect")).Return(3);
			_reader.Expect(r => r.GetDouble(3)).Return(123456D);
			_reader.Expect(r => r.GetOrdinal("DisplayColor")).Return(4);
			_reader.Expect(r => r.GetInt32(4)).Return(123456789);
			_reader.Expect(r => r.GetOrdinal("ActivityId")).Return(5).Repeat.Twice();
			_reader.Expect(r => r.IsDBNull(5)).Return(false);
			_reader.Expect(r => r.GetGuid(5)).Return(_guid);
			_reader.Expect(r => r.GetOrdinal("ThresholdTime")).Return(6);
			_reader.Expect(r => r.GetInt64(6)).Return(123456789);
			_reader.Expect(r => r.GetOrdinal("Name")).Return(7);
			_reader.Expect(r => r.GetString(7)).Return("name");
			_reader.Expect(r => r.Read()).Return(false);
			_reader.Expect(r => r.Close());
			_connection.Expect(c => c.Dispose());
			_mock.ReplayAll();

			var result = _target.ActivityAlarms();
			_mock.VerifyAll();

			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void VerifyAddOrUpdate()
		{
			var agentState = new ActualAgentState();
			var dataParameters = _mock.StrictMock<IDataParameterCollection>();

			_stringHandler.Expect(sh => sh.DataStoreConnectionString()).Return("connectionString");
			_connectionFactory.Expect(cf => cf.CreateConnection("connectionString")).Return(_connection);
			_connection.Expect(c => c.CreateCommand()).Return(_command);

			_command.CommandType = CommandType.StoredProcedure;
			_command.CommandText = "[RTA].[rta_addorupdate_actualagentstate]";

			_command.Expect(c => c.Parameters).Return(dataParameters).Repeat.Any();
			dataParameters.Expect(d => d.Add(new SqlParameter())).IgnoreArguments().Return(0).Repeat.Any();
			
			_connection.Expect(c => c.Open());
			_command.Expect(c => c.ExecuteNonQuery()).Return(1);
			_connection.Expect(c => c.Dispose());
			_mock.ReplayAll();

			_target.AddOrUpdate(new List<IActualAgentState> {agentState});
			_mock.VerifyAll();
		}

		[Test]
		public void VerifyCurrentLayerAndNext()
		{
			var dateTime = DateTime.UtcNow;
			var scheduleLayers = new List<ScheduleLayer>
				{
					new ScheduleLayer
						{
							StartDateTime = dateTime.AddMinutes(-5),
							EndDateTime = dateTime.AddMinutes(5)
						},
					new ScheduleLayer
						{
							StartDateTime = dateTime.AddMinutes(5),
							EndDateTime = dateTime.AddMinutes(10)
						}
				};
			var result = _target.CurrentLayerAndNext(dateTime, scheduleLayers);
			Assert.That(result.First().StartDateTime, Is.EqualTo(dateTime.AddMinutes(-5)));
			Assert.That(result.Last().EndDateTime, Is.EqualTo(dateTime.AddMinutes(10)));
		}

		[Test]
		public void VerifyCurrentLayerAndNextNoLayerNow()
		{
			var dateTime = DateTime.UtcNow;
			var scheduleLayers = new List<ScheduleLayer>
				{
					new ScheduleLayer
						{
							StartDateTime = dateTime.AddMinutes(5),
							EndDateTime = dateTime.AddMinutes(10)
						},
					new ScheduleLayer
						{
							StartDateTime = dateTime.AddMinutes(10),
							EndDateTime = dateTime.AddMinutes(20)
						}
				};
			var result = _target.CurrentLayerAndNext(dateTime, scheduleLayers);
			Assert.IsNull(result.First());
			Assert.That(result.Last().EndDateTime, Is.EqualTo(dateTime.AddMinutes(10)));
		}

		[Test]
		public void VerifyCurrentLayerAndNextLastAssignment()
		{
			var dateTime = DateTime.UtcNow;
			var scheduleLayers = new List<ScheduleLayer>
				{
					new ScheduleLayer
						{
							StartDateTime = dateTime.AddMinutes(-5),
							EndDateTime = dateTime.AddMinutes(5)
						},
					new ScheduleLayer
						{
							StartDateTime = dateTime.AddDays(1),
							EndDateTime = dateTime.AddDays(1)
						}
				};
			var result = _target.CurrentLayerAndNext(dateTime, scheduleLayers);
			Assert.IsNull(result.Last());
		}
	}
}