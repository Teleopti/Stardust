using System;
using System.Data.SqlClient;
using System.Reflection;

namespace Teleopti.Ccc.TestCommon
{
	public static class SqlExceptionConstructor
	{
		private static T Construct<T>(Type[] t, params object[] p)
		{
			return (T)typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, t, null).Invoke(p);
		}

		public static SqlException CreateSqlException(string errorMessage, int errorNumber)
		{
			SqlErrorCollection collection = Construct<SqlErrorCollection>(new Type[] { });

			var errorTypes = new[]
			                 	{
			                 		typeof (int), typeof (byte), typeof (byte), typeof (string), typeof (string), typeof (string),
			                 		typeof (int)
			                 	};
			SqlError error1 =
				Construct<SqlError>(errorTypes, errorNumber, (byte)2, (byte)3, "server name", errorMessage, "proc", 100);

			SqlError error2 =
				Construct<SqlError>(errorTypes, errorNumber+1, (byte)2, (byte)3, "server name", errorMessage+"2", "proc", 100);

			var addMethod = collection.GetType().GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
			addMethod.Invoke(collection, new object[] { error1 });
			addMethod.Invoke(collection, new object[] { error2 });

			var types = new[] { typeof(string), typeof(SqlErrorCollection) };
			var parameters = new object[] { "mess", collection };

			var constructor = typeof(SqlException).
				GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

			if (constructor == null)
			{
				types = new[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) };
				parameters = new object[] { "mess", collection, null, Guid.NewGuid() };

				constructor = typeof(SqlException).
					GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
			}

			var exception = (SqlException)constructor.Invoke(parameters);

			return exception;
		}
	}
}