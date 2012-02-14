using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	[Serializable]
	public class PositiveTimeSpanOrNullMappingType : IUserType
	{
		private static readonly SqlType[] sqlTypes = new[] { new SqlType(DbType.Int64) };

		public new bool Equals(object x, object y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}

			var xTimeSpan = (PositiveTimeSpan)x;
			var yTimeSpan = (PositiveTimeSpan)y;
			return xTimeSpan.Equals(yTimeSpan);
		}

		public int GetHashCode(object x)
		{
			return x.GetHashCode();
		}

		public object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			var theTimeSpan = NHibernateUtil.TimeSpan.NullSafeGet(rs, names);
			if (theTimeSpan == null)
				return null;
			return new PositiveTimeSpan((TimeSpan)theTimeSpan);
		}

		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			if(value is PositiveTimeSpan)
			{
				var casted = (PositiveTimeSpan) value;
				NHibernateUtil.TimeSpan.NullSafeSet(cmd, casted.TimeSpan, index);
			}
			else
			{
				((IDataParameter)cmd.Parameters[index]).Value = DBNull.Value;				
			}
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
			get
			{
				return sqlTypes;
			}
		}

		public Type ReturnedType
		{
			get { return typeof(PositiveTimeSpan); }
		}

		public bool IsMutable
		{
			get { return true; }
		}
	}
}