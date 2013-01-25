using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using log4net;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class PersonResolverTest
	{
		private IPersonResolver _target;
		private MockRepository _mock;
		private ILog _loggingSvc;
		private IDatabaseConnectionFactory _databaseConnectionFactory;
		private string _connectionString;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_loggingSvc = _mock.DynamicMock<ILog>();
			_databaseConnectionFactory = _mock.StrictMock<IDatabaseConnectionFactory>();
			_connectionString = "connection";
		}

		[Test]
		public void ShouldCreateNewInstace()
		{
			_target = new PersonResolver(_databaseConnectionFactory, _connectionString);
			Assert.That(_target, Is.Not.Null);
		}

		[Test]
		public void ShouldOnlyLoadDataOnce()
		{
			var connection = _mock.StrictMock<IDbConnection>();
			var command = _mock.StrictMock<IDbCommand>();
			var dataReader = _mock.StrictMock<IDataReader>();
			connection.Dispose();

			_databaseConnectionFactory.Expect(d => d.CreateConnection(_connectionString)).Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);

			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "RTA.rta_load_external_logon";
			connection.Open();

			command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
			dataReader.Expect(d => d.Read()).Return(true);
			
			dataReader.Expect(d => d.GetOrdinal("datasource_id")).Return(0);
			dataReader.Expect(d => d.GetInt16(0)).Return(2);
			dataReader.Expect(d => d.GetOrdinal("acd_login_original_id")).Return(1);
			dataReader.Expect(d => d.GetString(1)).Return("007");
			dataReader.Expect(d => d.GetOrdinal("person_code")).Return(2);
			dataReader.Expect(d => d.GetGuid(2)).Return(Guid.NewGuid());
			dataReader.Expect(d => d.GetOrdinal("business_unit_code")).Return(3);
			dataReader.Expect(d => d.GetGuid(3)).Return(Guid.NewGuid());

			dataReader.Expect(d => d.Read()).Return(false);
			dataReader.Close();
			_mock.ReplayAll();

			_target = new PersonResolverForTest(_databaseConnectionFactory, _connectionString, _loggingSvc);
			IEnumerable<PersonWithBusinessUnit> resolvedList;
			Assert.That(_target.TryResolveId(2, "007", out resolvedList), Is.True);
			Assert.That(resolvedList.Count(), Is.EqualTo(1));
			Assert.That(_target.TryResolveId(2, "007", out resolvedList), Is.True);
			Assert.That(resolvedList.Count(), Is.EqualTo(1));
			_mock.VerifyAll();
		}



		public class PersonResolverForTest : PersonResolver
		{
			public PersonResolverForTest(IDatabaseConnectionFactory databaseConnectionFactory,
			                             string connectionString, ILog loggingSvc)
				: base(databaseConnectionFactory, connectionString, loggingSvc)
			{
				
			}
		}
	}
}
