using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
	[IsNotDeadCode("Used in NH mapping files.")]
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

		/// <summary>
		/// Nulls the safe get.
		/// </summary>
		/// <param name="rs">The rs.</param>
		/// <param name="names">The names.</param>
		/// <param name="owner">The owner.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-20
		/// </remarks>
		public object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			object value = NHibernateUtil.DateTime.NullSafeGet(rs, names);
			if (value == null || value == DBNull.Value)
			{
				return null;
			}

			return new DateOnly((DateTime)value);
		}

		/// <summary>
		/// Nulls the safe set.
		/// </summary>
		/// <param name="cmd">The CMD.</param>
		/// <param name="value">The value.</param>
		/// <param name="index">The index.</param>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-20
		/// </remarks>
		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			object date = value;
			if (date != null)
			{
				if (date is DateOnly)
					date = ((DateOnly)value).Date;
			}
			NHibernateUtil.DateTime.NullSafeSet(cmd, date, index);
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
}