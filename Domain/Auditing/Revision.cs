﻿using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class Revision : IRevision, IEquatable<IRevision>
	{
		private int? HashCode { get; set; }
		public virtual long Id { get; set; }
		public virtual DateTime ModifiedAt { get; protected set; }
		public virtual IPerson ModifiedBy { get; protected set; }

		public virtual void SetRevisionData(IPerson currentUser)
		{
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
			return obj is IRevision rev && Equals(rev);
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