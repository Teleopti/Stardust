using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	[IsNotDeadCode("Used in NH mapping files.")]
	public class StringSavedAsDateTime : BaseImmutableUserType<string>
	{
		public override SqlType[] SqlTypes
		{
			get { return new[] {new SqlType(DbType.DateTime)}; }
		}

		public override object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			return DateTime.Parse(NHibernateUtil.String.NullSafeGet(rs, names[0]).ToString());
		}

		public override void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			DateTime valueToSet;
			if (DateTime.TryParse(value.ToString(), out valueToSet))
				NHibernateUtil.Decimal.NullSafeSet(cmd, valueToSet, index);
			else
				NHibernateUtil.Decimal.NullSafeSet(cmd, DBNull.Value, index);
		}
	}




	public abstract class BaseImmutableUserType<T> : IUserType
	{
		public abstract object NullSafeGet(IDataReader rs, string[] names, object owner);
		public abstract void NullSafeSet(IDbCommand cmd, object value, int index);
		public abstract SqlType[] SqlTypes { get; }

		public new bool Equals(object x, object y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x == null || y == null)
			{
				return false;
			}

			return x.Equals(y);
		}

		public int GetHashCode(object x)
		{
			return x.GetHashCode();
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
			return DeepCopy(cached);
		}

		public object Disassemble(object value)
		{
			return DeepCopy(value);
		}

		public Type ReturnedType
		{
			get { return typeof(T); }
		}

		public bool IsMutable
		{
			get { return false; }
		}
	}
}