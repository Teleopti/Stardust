using System;
using System.Data.SqlClient;

namespace Stardust.Manager.Extensions
{
	public static class SqlDataReaderExtensions
	{
		public static DateTime? GetNullableDateTime(this SqlDataReader reader, string fieldName)
		{
			var x = reader.GetOrdinal(fieldName);

			return reader.IsDBNull(x) ? (DateTime?) null : reader.GetDateTime(x);
		}

		public static DateTime? GetNullableDateTime(this SqlDataReader reader, int ordinalPosition)
		{
			return reader.IsDBNull(ordinalPosition) ? (DateTime?) null : reader.GetDateTime(ordinalPosition);
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