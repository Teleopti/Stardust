using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public interface ICryptoKeyInfoRepository
	{
		CryptoKeyInfo Find(string bucket, string handle);
		IEnumerable<CryptoKeyInfo> Find(string bucket);
		void Add(CryptoKeyInfo cryptoKeyInfo);
		void Remove(string bucket, string handle);
	}
}