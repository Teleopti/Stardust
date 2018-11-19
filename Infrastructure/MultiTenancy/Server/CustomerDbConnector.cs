using System;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface ICustomerDbConnector
	{
		bool TryGetEmailAddress(PersonInfo pi, out string emailAddress);
	}

	public class CustomerDbConnector : ICustomerDbConnector
	{
		public bool TryGetEmailAddress(PersonInfo pi, out string emailAddress)
		{
			using (var con = new SqlConnection(pi.Tenant.DataSourceConfiguration.ApplicationConnectionString))
			using (var cmd = new SqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; SELECT Email FROM Person WHERE Id = @userId", con)) // set transaction isolation level read uncommitted;
			{
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add("@userId", SqlDbType.UniqueIdentifier);
				cmd.Parameters["@userId"].Value = pi.Id;
				try
				{
					con.Open();
					emailAddress = cmd.ExecuteScalar().ToString();
					return true;
				}
				catch (Exception ex)
				{
					var asd = ex.Message;
					emailAddress = null;
					return false;
				}
			}
		}
	}
}