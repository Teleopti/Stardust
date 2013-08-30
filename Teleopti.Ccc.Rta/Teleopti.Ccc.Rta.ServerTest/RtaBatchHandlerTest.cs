using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class RtaBatchHandlerTest
	{
		private RtaBatchHandler _target;

		private IDatabaseConnectionFactory _databaseConnectionFactory;

		[SetUp]
		public void Setup()
		{
			_databaseConnectionFactory = MockRepository.GenerateMock<IDatabaseConnectionFactory>();
		
			_target = new RtaBatchHandler(_databaseConnectionFactory);
		}
		
		[Test]
		public void ShouldReturnDictionary()
		{
			var connection = MockRepository.GenerateMock<IDbConnection>();
			var command = MockRepository.GenerateMock<IDbCommand>();
			var dataReader = MockRepository.GenerateMock<IDataReader>();
			connection.Dispose();

			_databaseConnectionFactory.Expect(d => d.CreateConnection(""))
			                          .IgnoreArguments()
			                          .Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);

			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "RTA.rta_load_external_logon";
			connection.Open();

			command.Expect(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(dataReader);
			dataReader.Expect(d => d.Read()).Return(true).Repeat.Twice();

			dataReader.Expect(d => d.GetOrdinal("datasource_id")).Return(0).Repeat.Twice();
			dataReader.Expect(d => d.GetInt16(0)).Return(0).Repeat.Twice();
			dataReader.Expect(d => d.GetOrdinal("acd_login_original_id")).Return(1).Repeat.Twice();
			dataReader.Expect(d => d.GetString(1)).Return("0001");
			dataReader.Expect(d => d.GetString(1)).Return("0002");

			dataReader.Expect(d => d.Read()).Return(false);
			dataReader.Close();

			var result = _target.PeopleOnDataSource(0);
			result.Count.Should().Be.EqualTo(2);
		}


	}
}
