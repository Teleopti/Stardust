using System;
using System.Data;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public static class SqlDataReaderExtensions
	{

		public static Guid Guid(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return reader.GetGuid(ordinal);
		}

		public static T? NullableEnum<T>(this IDataReader reader, string name)
			where T: struct
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? (T) Enum.ToObject(typeof(T), reader.GetInt32(ordinal))
				: default(T);
		}

		public static int? NullableInt(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetInt32(ordinal)
				: (int?)null;
		}

		public static double? NullableDouble(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetDouble(ordinal)
				: (double?)null;
		}

		public static string String(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetString(ordinal)
				: null;
		}

		public static Guid? NullableGuid(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetGuid(ordinal)
				: (Guid?)null;
		}

		public static DateTime? NullableDateTime(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetDateTime(ordinal)
				: (DateTime?)null;
		}

		public static DateTime DateTime(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return reader.GetDateTime(ordinal);
		}

		public static bool Boolean(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return reader.GetBoolean(ordinal);
		}
	}
}