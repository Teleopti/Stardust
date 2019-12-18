using System;
using System.Data.SqlClient;

namespace Manager.IntegrationTest.Database
{
	public static class SqlDataReaderExtensions
	{
		public static DateTime? GetNullableDateTime(this SqlDataReader reader, int ordinalPosition)
		{
			return reader.IsDBNull(ordinalPosition) ? (DateTime?)null : reader.GetDateTime(ordinalPosition);
		}

		public static string GetNullableString(this SqlDataReader reader, int ordinalPosition)
		{
			if (reader.IsDBNull(ordinalPosition))
			{
				return null;
			}

			return reader.GetString(ordinalPosition);
		}
	}
}
