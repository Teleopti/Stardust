using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class DateOnlyAsDateMappingType : IUserType
	{
		bool IUserType.Equals(object x, object y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}

			var xDateOnly = (DateOnly)x;
			var yDateOnly = (DateOnly)y;
			return xDateOnly.Equals(yDateOnly);
		}

		public int GetHashCode(object x)
		{
			return x.GetHashCode();
		}

		public object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			var value = NHibernateUtil.Date.NullSafeGet(rs, names);
			if (value == null || value == DBNull.Value)
			{
				return null;
			}

			return new DateOnly((DateTime)value);
		}

		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			var date = value;
			if (date != null)
			{
				if (date is DateOnly)
					date = ((DateOnly)value).Date;
			}
			NHibernateUtil.Date.NullSafeSet(cmd, date, index);
		}

		public object DeepCopy(object value)
		{
			return value; 
		}

		public object Replace(object original, object target, object owner)
		{
			return original;
		}

		public object Assemble(object cached, object owner)
		{
			return cached;
		}

		public object Disassemble(object value)
		{
			return value;
		}

		public SqlType[] SqlTypes
		{
			get { return new[] {new SqlType(DbType.Date)}; }
		}

		public Type ReturnedType
		{
			get { return typeof(DateOnly); }
		}

		public bool IsMutable
		{
			get { return true; }
		}
	}
}