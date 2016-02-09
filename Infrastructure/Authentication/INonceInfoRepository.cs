using System;
using NHibernate;
using Teleopti.Ccc.Domain.Authentication;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public interface INonceInfoRepository
	{
		NonceInfo Find(string context, string nonce, DateTime timestamp);
		void Add(NonceInfo nonceInfo);
	}
}