using System;
using System.Collections.Generic;
using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OpenId;
using Teleopti.Ccc.Domain.Authentication;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public interface ICryptoKeyInfoRepository
	{
		CryptoKeyInfo Find(string bucket, string handle);
		IEnumerable<CryptoKeyInfo> Find(string bucket);
	}

	public class NonStandardProviderApplicationStore : IOpenIdApplicationStore, ICryptoKeyStore, INonceStore
	{
		private readonly ICryptoKeyInfoRepository _cryptoKeyInfoRepository;

		public NonStandardProviderApplicationStore(ICryptoKeyInfoRepository cryptoKeyInfoRepository)
		{
			_cryptoKeyInfoRepository = cryptoKeyInfoRepository;
		}

		public CryptoKey GetKey(string bucket, string handle)
		{
			var cryptoKeyInfo = _cryptoKeyInfoRepository.Find(bucket,handle);
			return cryptoKeyInfo == null ? null : new CryptoKey(cryptoKeyInfo.CryptoKey, cryptoKeyInfo.CryptoKeyExpiration);
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
		{
			throw new NotImplementedException();
		}

		public void StoreKey(string bucket, string handle, CryptoKey key)
		{
			return;
		}

		public void RemoveKey(string bucket, string handle)
		{
			return;
		}

		public bool StoreNonce(string context, string nonce, DateTime timestampUtc)
		{
			return true;
		}
	}
}