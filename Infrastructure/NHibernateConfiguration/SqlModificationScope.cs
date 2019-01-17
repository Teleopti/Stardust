using System;
using NHibernate.SqlCommand;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class SqlModificationScope
	{
		[ThreadStatic]
		private static IModifySql _modifySql;

		public static IDisposable Create(IModifySql modifySql)
		{
			_modifySql = modifySql;
			return new GenericDisposable(() => _modifySql = null);
		}

		public static IModifySql Current()
		{
			return _modifySql ?? new noSqlModification();
		}
		
		private class noSqlModification : IModifySql
		{
			public SqlString Execute(SqlString sqlString)
			{
				return sqlString;
			}
		}
	}
}