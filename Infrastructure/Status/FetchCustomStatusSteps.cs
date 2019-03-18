using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.Infrastructure.Status
{
	public class FetchCustomStatusSteps : IFetchCustomStatusSteps
	{
		private readonly Lazy<string> _connectionString;
		private const string baseSql = 
			"select id, name, description, secondslimit, datediff(second, lastping, getdate()) as secondslastping, canbedeleted from status.CustomStatusStep";

		public FetchCustomStatusSteps(IConfigReader config)
		{
			_connectionString = new Lazy<string>(() => config.ConnectionString("Status"));
		}
		
		public IEnumerable<CustomStatusStep> Execute()
		{
			using (var conn = new SqlConnection(_connectionString.Value))
			{
				conn.Open();
				using(var cmd = new SqlCommand(baseSql, conn))
				{
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						yield return createInstance(reader);
					}
				}
			}
		}

		public CustomStatusStep Execute(string name)
		{
			using (var conn = new SqlConnection(_connectionString.Value))
			{
				conn.Open();

				var param = new SqlParameter {ParameterName = "@name", Value = name};

				using(var cmd = new SqlCommand(baseSql + " where name=@name", conn))
				{
					cmd.Parameters.Add(param);
					var reader = cmd.ExecuteReader();
					return reader.Read() ? 
						createInstance(reader) : 
						null;
				}
			}
		}
		
		private static CustomStatusStep createInstance(IDataRecord reader)
		{
			return new CustomStatusStep(reader.GetInt32(reader.GetOrdinal("id")),
				reader.GetString(reader.GetOrdinal("name")), 
				reader.GetString(reader.GetOrdinal("description")),
				TimeSpan.FromSeconds(reader.GetInt32(reader.GetOrdinal("secondslastping"))), 
				TimeSpan.FromSeconds(reader.GetInt32(reader.GetOrdinal("secondslimit"))),
				reader.GetBoolean(reader.GetOrdinal("canbedeleted")));
		}
	}
}