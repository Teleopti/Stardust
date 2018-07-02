using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	/// <summary>
	/// Base class for all entities.
	/// </summary>
	public abstract class Entity : IEntity
	{
#pragma warning disable 0649
		private Guid? _id;
#pragma warning restore 0649

		private int? _hashCode;

		#region IEntity Members

		/// <summary>
		/// Gets the unique id for this entity.
		/// </summary>
		/// <value>The id.</value>
		public virtual Guid? Id => _id;


		/// <summary>
		/// Sets the id.
		/// </summary>
		/// <param name="newId">The new ID.</param>
		public virtual void SetId(Guid? newId)
		{
			if (newId.HasValue)
			{
				_id = newId;
			}
			else
			{
				ClearId();
			}
		}

		public virtual void ClearId()
		{
			_id = null;
		}


		#endregion

		#region Object methods overriding

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			string typeName = GetType().Name;
			if (Id.HasValue)
				return String.Concat(typeName, ", ", Id.Value);
			return String.Concat(typeName, ", no id");
		}


		/// <summary>
		/// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override int GetHashCode()
		{
			if (!_hashCode.HasValue)
			{
				_hashCode = _id.HasValue ? _id.Value.GetHashCode() : base.GetHashCode();
			}
			return _hashCode.Value;
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is IEntity ent && Equals(ent);
		}

		///<summary>
		///Indicates whether the current object is equal to another object of the same type.
		///</summary>
		///
		///<returns>
		///true if the current object is equal to the other parameter; otherwise, false.
		///</returns>
		///
		///<param name="other">An object to compare with this object.</param>
		public virtual bool Equals(IEntity other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;
			if (!other.Id.HasValue || !Id.HasValue)
				return false;

			return Id.Value == other.Id.Value;
		}

		#endregion
	}
}