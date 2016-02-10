using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OpenId;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	public class SqlProviderApplicationStore : IOpenIdApplicationStore
	{
		private readonly ICryptoKeyInfoRepository _cryptoKeyInfoRepository;
		private readonly INonceInfoRepository _nonceInfoRepository;
		private readonly INow _now;
		private readonly TimeSpan maximumMessageAge = TimeSpan.FromMinutes(5);

		public SqlProviderApplicationStore(ICryptoKeyInfoRepository cryptoKeyInfoRepository, INonceInfoRepository nonceInfoRepository, INow now)
		{
			_cryptoKeyInfoRepository = cryptoKeyInfoRepository;
			_nonceInfoRepository = nonceInfoRepository;
			_now = now;
		}

		public CryptoKey GetKey(string bucket, string handle)
		{
			var cryptoKeyInfo = _cryptoKeyInfoRepository.Find(bucket, handle);
			return cryptoKeyInfo == null ? null : new CryptoKey(cryptoKeyInfo.CryptoKey, DateTime.SpecifyKind(cryptoKeyInfo.CryptoKeyExpiration, DateTimeKind.Utc));
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
		{
			
			return _cryptoKeyInfoRepository.Find(bucket)
				.Select(x => new KeyValuePair<string, CryptoKey>(x.Handle, new CryptoKey(x.CryptoKey, DateTime.SpecifyKind(x.CryptoKeyExpiration, DateTimeKind.Utc))));
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

		public bool StoreNonce(string context, string nonce, DateTime timestamp)
		{
			if (toUniversalTimeSafe(timestamp) + maximumMessageAge < _now.UtcDateTime())
				return false;
			var nonceInfo = _nonceInfoRepository.Find(context, nonce, timestamp);
			if (nonceInfo == null)
			{
                _nonceInfoRepository.ClearExpired(_now.UtcDateTime().Subtract(maximumMessageAge));
                _nonceInfoRepository.Add(new NonceInfo
				{
					Context = context,
					Nonce = nonce,
					Timestamp = timestamp
				});
				return true;
			}
			return false;
		}

		private static DateTime toUniversalTimeSafe(DateTime value)
		{
			return value.Kind == DateTimeKind.Unspecified ? value : value.ToUniversalTime();
		}
	}
}