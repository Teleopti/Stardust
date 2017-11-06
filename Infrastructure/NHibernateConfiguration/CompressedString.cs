using System;
using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class CompressedString : IUserType
	{
		public object Assemble(object cached, object owner)
		{
			throw new NotImplementedException();
		}

		public object DeepCopy(object value)
		{
			return value == null ? null : string.Copy((string)value);
		}

		public object Disassemble(object value)
		{
			throw new NotImplementedException();
		}

		public new bool Equals(object x, object y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(object x)
		{
			if (x == null)
			{
				throw new ArgumentNullException(nameof(x));
			}
			return x.GetHashCode();
		}

		public bool IsMutable => true;

		public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			if (names.Length != 1)
				throw new InvalidOperationException("names array has more than one element. can't handle this!");

			var valueToGet = (string)NHibernateUtil.String.NullSafeGet(rs, names[0], session);
			return valueToGet?.ToUncompressedString();
		}

		public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
		{
			if (value == null)
			{
				NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
				return;
			}

			var valueToSet = (string)value;
			NHibernateUtil.String.NullSafeSet(cmd, valueToSet.ToCompressedBase64String(), index, session);
		}

		public object Replace(object original, object target, object owner)
		{
			throw new NotImplementedException();
		}

		public Type ReturnedType => typeof(string);

		public SqlType[] SqlTypes => new[] { new SqlType(DbType.String) };
	}
}