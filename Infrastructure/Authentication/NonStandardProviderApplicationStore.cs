using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OpenId;
using Teleopti.Ccc.Domain.Authentication;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class NonStandardProviderApplicationStore : IOpenIdApplicationStore, ICryptoKeyStore, INonceStore
	{
		private readonly ICryptoKeyInfoRepository _cryptoKeyInfoRepository;

		public NonStandardProviderApplicationStore(ICryptoKeyInfoRepository cryptoKeyInfoRepository, INonceInfoRepository nonceInfoRepository)
		{
			_cryptoKeyInfoRepository = cryptoKeyInfoRepository;
		}

		public CryptoKey GetKey(string bucket, string handle)
		{
			var cryptoKeyInfo = _cryptoKeyInfoRepository.Find(bucket, handle);
			return cryptoKeyInfo == null ? null : new CryptoKey(cryptoKeyInfo.CryptoKey, cryptoKeyInfo.CryptoKeyExpiration);
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
		{
			return _cryptoKeyInfoRepository.Find(bucket)
				.Select(x => new KeyValuePair<string, CryptoKey>(x.Handle, new CryptoKey(x.CryptoKey, x.CryptoKeyExpiration)));
		}

		public void StoreKey(string bucket, string handle, CryptoKey key)
		{
			_cryptoKeyInfoRepository.Add(new CryptoKeyInfo
			{
				Bucket = bucket,
				Handle = handle,
				CryptoKey = key.Key,
				CryptoKeyExpiration = key.ExpiresUtc
			});
		}

		public void RemoveKey(string bucket, string handle)
		{
			_cryptoKeyInfoRepository.Remove(bucket, handle);
		}

		public bool StoreNonce(string context, string nonce, DateTime timestampUtc)
		{
			return true;
		}
	}
}