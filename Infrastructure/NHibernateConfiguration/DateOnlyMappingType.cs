using System;
using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	/// <summary>
	/// DateOnly NHibernate Mapping Type
	/// Implemented with help from http://www.hibernate.org/368.html example.
	/// </summary>
	/// <remarks>
	/// Created by: HenryG
	/// Created date: 2008-11-20
	/// </remarks>
	public class DateOnlyMappingType : IUserType
	{
		#region Implementation of IUserType

		private static readonly SqlType[] _sqlTypes = new[] { new SqlType(DbType.Date) };

		/// <summary>
		/// Equalses the specified x.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-19
		/// </remarks>
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

			DateOnly xDateOnly = (DateOnly)x;
			DateOnly yDateOnly = (DateOnly)y;
			return xDateOnly.Equals(yDateOnly);
		}

		/// <summary>
		/// Gets the hash code.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		public int GetHashCode(object x)
		{
			DateOnly date = (DateOnly)x;
			int hashCode = 1;
			unchecked
			{
				hashCode = 31 * hashCode + date.Day;
				hashCode = 31 * hashCode + date.Month;
				hashCode = 31 * hashCode + date.Year;
			}
			return hashCode;
		}

		public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			object value = NHibernateUtil.DateTime.NullSafeGet(rs, names, session);
			if (value == null || value == DBNull.Value)
			{
				return null;
			}

			return new DateOnly((DateTime)value);
		}

		public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
		{
			object date = value;
			if (date is DateOnly dateValue)
				date = dateValue.Date;
			NHibernateUtil.DateTime.NullSafeSet(cmd, date, index, session);
		}

		/// <summary>
		/// Deeps the copy.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-19
		/// </remarks>
		public object DeepCopy(object value)
		{
			return value; //As this is a struct a copy will be returned
		}

		/// <summary>
		/// Replaces the specified original.
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="target">The target.</param>
		/// <param name="owner">The owner.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		public object Replace(object original, object target, object owner)
		{
			return original;
		}

		/// <summary>
		/// Assembles the specified cached.
		/// </summary>
		/// <param name="cached">The cached.</param>
		/// <param name="owner">The owner.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		public object Assemble(object cached, object owner)
		{
			return cached;
		}

		public object Disassemble(object value)
		{
			return value;
		}

		/// <summary>
		/// Gets the SQL types.
		/// </summary>
		/// <value>The SQL types.</value>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-19
		/// </remarks>
		public SqlType[] SqlTypes
		{
			get { return _sqlTypes; }
		}

		/// <summary>
		/// Gets the type of the returned.
		/// </summary>
		/// <value>The type of the returned.</value>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-19
		/// </remarks>
		public Type ReturnedType
		{
			get { return typeof(DateOnly); }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is mutable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is mutable; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-19
		/// </remarks>
		public bool IsMutable
		{
			get { return true; }
		}

		#endregion
	}

	public class NullableDateTimePeriodCompositeUserType : ICompositeUserType
	{

		public Type ReturnedClass
		{
			get { return typeof (DateTimePeriod?); }
		}

		public new bool Equals(object x, object y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			return x.Equals(y);
		}

		public int GetHashCode(object x)
		{
			var obj = x as DateTimePeriod?;
			return obj.GetHashCode();
		}

		public object DeepCopy(object value)
		{
			return value;
		}

		public bool IsMutable
		{
			get { return false; }
		}

		public object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner)
		{
			object obj0 = NHibernateUtil.UtcDateTime.NullSafeGet(dr, names[0], session);
			object obj1 = NHibernateUtil.UtcDateTime.NullSafeGet(dr, names[1], session);
			if (obj0 == null || obj1 == null) return null;
			var start = (DateTime) obj0;
			var end = (DateTime) obj1;
			return new DateTimePeriod(start, end);
		}

		public void NullSafeSet(DbCommand cmd, object obj, int index, bool[] settable, ISessionImplementor session)
		{
			var period = (DateTimePeriod?) obj;
			if (!period.HasValue)
			{
				((IDataParameter) cmd.Parameters[index]).Value = DBNull.Value;
				((IDataParameter) cmd.Parameters[index + 1]).Value = DBNull.Value;
			}
			else
			{
				((IDataParameter) cmd.Parameters[index]).Value = period.Value.StartDateTime;
				((IDataParameter) cmd.Parameters[index + 1]).Value = period.Value.EndDateTime;
			}
		}

		public object Replace(object original, object target, ISessionImplementor session, object owner)
		{
			throw new NotImplementedException();
		}

		public string[] PropertyNames
		{
			get { return new[] {"Start", "End"}; }
		}

		public IType[] PropertyTypes
		{
			get
			{
				return new IType[]
				{
					NHibernateUtil.UtcDateTime, NHibernateUtil.UtcDateTime
				};
			}
		}

		public object GetPropertyValue(object component, int property)
		{
			var period = (DateTimePeriod?) component;
			if (!period.HasValue) return null;
			if (property == 0)
				return period.Value.StartDateTime;
			return period.Value.EndDateTime;
		}

		public void SetPropertyValue(object comp, int property, object value)
		{
			throw new Exception("Immutable!");
		}

		public object Assemble(object cached,
			ISessionImplementor session, object owner)
		{
			return cached;
		}

		public object Disassemble(object value,
			ISessionImplementor session)
		{
			return value;
		}
	}
}