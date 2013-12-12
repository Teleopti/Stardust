using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Resolvers;

namespace Teleopti.Ccc.Rta.ServerTest.Resolvers
{
	[TestFixture]
	public class PersonResolverTest
	{
		private IPersonResolver _target;
		private MockRepository _mock;

		private readonly Guid _personId = Guid.NewGuid();
		private readonly Guid _businessUnitId = Guid.NewGuid();
		private readonly Guid _secondBusinessUnit = Guid.NewGuid();
		private IDatabaseReader _databaseReader;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_databaseReader = _mock.DynamicMock<IDatabaseReader>();
			_target = new PersonResolver(_databaseReader);
		}

		[Test]
		public void ShouldReturnEmptyGuidWhenLogOnIsEmpty()
		{
			IEnumerable<PersonWithBusinessUnit> resolvedList;
			Assert.That(_target.TryResolveId(2, string.Empty, out resolvedList), Is.True);
			Assert.That(resolvedList.Count(p => p.BusinessUnitId == Guid.Empty && p.PersonId == Guid.Empty), Is.EqualTo(1));
		}

		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
		//public void ShouldAddPersonWithBusinessUnitToListWhenLoopedTwice()
		//{
		//	var connection = _mock.StrictMock<IDbConnection>();
		//	var command = _mock.StrictMock<IDbCommand>();
		//	var dataReader = _mock.StrictMock<IDataReader>();
		//	connection.Dispose();

		//	_databaseConnectionFactory.Expect(d => d.CreateConnection(_connectionString)).Return(connection);
		//	connection.Expect(c => c.CreateCommand()).Return(command);

		//	command.CommandType = CommandType.StoredProcedure;
		//	command.CommandText = "RTA.rta_load_external_logon";
		//	connection.Open();

		//	command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
		//	dataReader.Expect(d => d.Read()).Return(true).Repeat.Twice();

		//	dataReader.Expect(d => d.GetOrdinal("datasource_id")).Return(0).Repeat.Twice();
		//	dataReader.Expect(d => d.GetInt16(0)).Return(2).Repeat.Twice();
		//	dataReader.Expect(d => d.GetOrdinal("acd_login_original_id")).Return(1).Repeat.Twice();
		//	dataReader.Expect(d => d.GetString(1)).Return("A0001").Repeat.Twice();
		//	dataReader.Expect(d => d.GetOrdinal("person_code")).Return(2).Repeat.Twice();
		//	dataReader.Expect(d => d.GetGuid(2)).Return(_personId).Repeat.Twice();
		//	dataReader.Expect(d => d.GetOrdinal("business_unit_code")).Return(3).Repeat.Twice();
		//	dataReader.Expect(d => d.GetGuid(3)).Return(_businessUnitId);
		//	dataReader.Expect(d => d.GetGuid(3)).Return(_secondBusinessUnit);

		//	dataReader.Expect(d => d.Read()).Return(false);
		//	dataReader.Close();
		//	_mock.ReplayAll();

		//	IEnumerable<PersonWithBusinessUnit> resolvedList;
		//	Assert.That(_target.TryResolveId(2, "a0001", out resolvedList), Is.True);
		//	resolvedList = resolvedList.ToList();
		//	Assert.That(resolvedList.Count(p => p.BusinessUnitId == _businessUnitId && p.PersonId == _personId), Is.EqualTo(1));
		//	Assert.That(resolvedList.Count(p => p.BusinessUnitId == _secondBusinessUnit && p.PersonId == _personId), Is.EqualTo(1));
		//	_mock.VerifyAll();
		//}
	}
}
