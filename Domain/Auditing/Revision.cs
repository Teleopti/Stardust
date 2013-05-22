using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class Revision : IRevision, IEquatable<IRevision>
	{
		private int? HashCode { get; set; }
		public virtual long Id { get; set; }
		public virtual DateTime ModifiedAt { get; protected set; }
		public virtual IPerson ModifiedBy { get; protected set; }

		public virtual void SetRevisionData(IPerson currentUser, DateTime utcNow)
		{
			ModifiedAt = utcNow;
			ModifiedBy = currentUser;
		}

		public override int GetHashCode()
		{
			if (!HashCode.HasValue)
			{
				HashCode = Id > 0 ? Id.GetHashCode() : 0;
			}
			return HashCode.Value;
		}

		public override bool Equals(object obj)
		{
			var rev = obj as IRevision;
			return rev != null && Equals(rev);
		}


		public virtual bool Equals(IRevision other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;

			return (Id == other.Id);
		}
	}
}