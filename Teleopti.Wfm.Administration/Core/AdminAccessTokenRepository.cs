using System;
using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Wfm.Administration.Core
{
	public class AdminAccessTokenRepository
	{
		private readonly IHashFunction _hashFunction;
		private readonly INow _now;
		private static readonly TimeSpan tokenTimeToLive = TimeSpan.FromMinutes(40); 

		public AdminAccessTokenRepository(IHashFunction hashFunction, INow now)
		{
			_hashFunction = hashFunction;
			_now = now;
		}
		
		public static bool TokenIsValid(string token, INow now, IConfigReader config)
		{
			bool isValid;
			var builder = new SqlConnectionStringBuilder(config.ConnectionString("Tenancy"));
			using (var sqlConn = new SqlConnection(builder.ConnectionString))
			{
				sqlConn.Open();

				int cnt;
				using (var sqlCommand = new SqlCommand("SELECT COUNT(*) FROM Tenant.AdminAccessToken WHERE AccessToken = @AccessToken AND Expires >= @Now", sqlConn))
                {
	                sqlCommand.Parameters.AddWithValue("@AccessToken", token);
					sqlCommand.Parameters.AddWithValue("@Now", now.UtcDateTime());
					cnt = (int)sqlCommand.ExecuteScalar();
				}
				isValid = cnt > 0;
				
				if(isValid)
				{
					using (var sqlCommand = new SqlCommand("UPDATE Tenant.AdminAccessToken SET Expires = @NewExpiration WHERE AccessToken = @AccessToken", sqlConn))
					{
						sqlCommand.Parameters.AddWithValue("@AccessToken", token);
						sqlCommand.Parameters.AddWithValue("@NewExpiration", now.UtcDateTime().Add(tokenTimeToLive));
						sqlCommand.ExecuteNonQuery();
					}
				}
				sqlConn.Close();
			}
			return isValid;
		}

		public string CreateNewToken(int userId, SqlConnection sqlConnection)
		{
			string accessToken;

			using (var sqlCommand =
				new SqlCommand("DELETE FROM Tenant.AdminAccessToken WHERE Expires < @Now", sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@Now", _now.UtcDateTime());
				sqlCommand.ExecuteNonQuery();
			}
			
			using (var sqlCommand = new SqlCommand("INSERT INTO Tenant.AdminAccessToken VALUES (@UserId, @AccessToken, @Expires)", sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@UserId", userId);
				accessToken = createToken();
				sqlCommand.Parameters.AddWithValue("@AccessToken", accessToken);
				sqlCommand.Parameters.AddWithValue("@Expires", _now.UtcDateTime().Add(tokenTimeToLive));
				sqlCommand.ExecuteNonQuery();
			}
			
			return accessToken;
		}

		private string createToken()
		{
			return _hashFunction.CreateHash(Guid.NewGuid().ToString());
		}
	}
}