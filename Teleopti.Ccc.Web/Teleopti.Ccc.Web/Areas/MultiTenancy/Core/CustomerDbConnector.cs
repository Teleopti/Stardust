using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface ICustomerDbConnector
	{
		string TryGetEmailAddress(PersonInfo pi);
	}

	public class CustomerDbConnector : ICustomerDbConnector
	{
		public string TryGetEmailAddress(PersonInfo pi)
		{
			using (var con = new SqlConnection(pi.Tenant.DataSourceConfiguration.ApplicationConnectionString))
			using (var cmd = new SqlCommand("SELECT Email FROM Person WHERE Id = @userId", con))
			{
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add("@userId", SqlDbType.UniqueIdentifier);
				cmd.Parameters["@userId"].Value = pi.Id;
				try
				{
					con.Open();
					var emailAddress = cmd.ExecuteScalar();
					return emailAddress.ToString();
				}
				catch (Exception)
				{
					return null;
				}
			}
		}
	}
}