using System.Data.SqlClient;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class RtaDatabaseTestAttribute : DatabaseTestAttribute
	{
		protected override void AfterTest()
		{
			base.AfterTest();
			ApplySql("DELETE FROM RTA.ActualAgentState");
		}
		
		public static void ApplySql(string sql)
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			{
				connection.Open();
				using (var command = new SqlCommand(sql, connection))
					command.ExecuteNonQuery();
			}
		}

	}
}