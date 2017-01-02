using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Authentication;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeNonceInfoRepository : INonceInfoRepository
	{
		private readonly List<NonceInfo> nonceInfos = new List<NonceInfo>();

		public NonceInfo Find(string context, string nonce, DateTime timestamp)
		{
			return nonceInfos.FirstOrDefault(x => x.Timestamp == timestamp && x.Context == context && x.Nonce == nonce);
		}

		public void Add(NonceInfo nonceInfo)
		{
			if (nonceInfo.Id.HasValue) nonceInfos.RemoveAll(x => x.Id == nonceInfo.Id);
			nonceInfos.Add(nonceInfo);
		}

		public void ClearExpired(DateTime expiredTimestamp)
		{
			nonceInfos.RemoveAll(x => x.Timestamp < expiredTimestamp);
		}
	}
}