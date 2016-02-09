using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class NonceInfo : Entity
	{
		public virtual string Context { get; set; }
		public virtual string Nonce { get; set; }
		public virtual DateTime Timestamp { get; set; }
	}
}