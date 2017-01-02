using System;
using System.Linq;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Authentication;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeCryptoKeyInfoRepository : ICryptoKeyInfoRepository
	{
		private readonly List<CryptoKeyInfo> cryptoKeyInfos = new List<CryptoKeyInfo>();

		public CryptoKeyInfo Find(string bucket, string handle)
		{
			return cryptoKeyInfos.FirstOrDefault(x => x.Bucket == bucket && x.Handle == handle && x.CryptoKeyExpiration > DateTime.UtcNow);
		}

		public IEnumerable<CryptoKeyInfo> Find(string bucket)
		{
			return cryptoKeyInfos
				.Where(x => x.Bucket == bucket && x.CryptoKeyExpiration > DateTime.UtcNow)
				.OrderByDescending(x => x.CryptoKeyExpiration)
				.ToList();
		}

		public void Add(CryptoKeyInfo cryptoKeyInfo)
		{
			if (cryptoKeyInfo.Id.HasValue) cryptoKeyInfos.RemoveAll(x => x.Id == cryptoKeyInfo.Id);
			cryptoKeyInfos.Add(cryptoKeyInfo);
		}

		public void Remove(string bucket, string handle)
		{
			var cryptoKeyInfo = Find(bucket, handle);
			if (cryptoKeyInfo != null)
				cryptoKeyInfos.Remove(cryptoKeyInfo);
		}

		public void ClearExpired(DateTime expiredTimestamp)
		{
			cryptoKeyInfos.RemoveAll(x => x.CryptoKeyExpiration < expiredTimestamp);
		}
	}
}