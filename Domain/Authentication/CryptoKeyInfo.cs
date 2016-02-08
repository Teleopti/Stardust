using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Authentication
{
	public class CryptoKeyInfo: Entity
	{
		public virtual string Bucket { get; set; }
		public virtual string Handle { get; set; }
		public virtual byte[] CryptoKey { get; set; }
		public virtual DateTime CryptoKeyExpiration { get; set; }
	}
}