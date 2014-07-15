using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadge:AggregateRoot
	{
		private Guid _personId;
		private int _bronzeBadge;
		private int _silverBadge;
		private int _goldenBadge;

		public virtual Guid PersonId
		{
			get { return _personId; }
			set { _personId = value; }
		}

		public virtual int BronzeBadge
		{
			get { return _bronzeBadge; }
			set { _bronzeBadge = value; }
		}

		public virtual int SilverBadge
		{
			get { return _silverBadge; }
			set { _silverBadge = value; }
		}

		public virtual int GoldenBadge
		{
			get { return _goldenBadge; }
			set { _goldenBadge = value; }
		}
	}
}