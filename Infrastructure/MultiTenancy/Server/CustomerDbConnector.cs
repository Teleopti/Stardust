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
			using (var cmd = new SqlCommand("SELECT Email FROM Person WITH (READUNCOMMITTED) WHERE Id = @userId", con))
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
				catch (Exception)
				{
					emailAddress = null;
					return false;
				}
			}
		}
	}
}