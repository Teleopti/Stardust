using System;
using DotNetOpenAuth.Messaging.Bindings;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Auth.OpenIdApplicationStore;

namespace Teleopti.Ccc.Web.AuthTest.OpenIdApplicationStore
{
	[TestFixture]
	public class SqlProviderApplicationStoreTest
	{
		[Test]
		public void ShouldGetKey()
		{
			var cryptoKeyInfoRepository = new FakeCryptoKeyInfoRepository();
			var now = new Now();
			var cryptoKeyInfo = new CryptoKeyInfo
			{
				CryptoKey = Guid.NewGuid().ToByteArray(),
				CryptoKeyExpiration = now.UtcDateTime() + TimeSpan.FromMinutes(1),
				Bucket = "bucket",
				Handle = "handle",
			};
			cryptoKeyInfoRepository.Add(cryptoKeyInfo);
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, now);

			var cryptoKey = target.GetKey("bucket", "handle");

			cryptoKey.Should().Not.Be.Null();
			cryptoKey.Key.Should().Have.SameSequenceAs(cryptoKeyInfo.CryptoKey);
			cryptoKey.ExpiresUtc.Should().Be.EqualTo(cryptoKeyInfo.CryptoKeyExpiration);
		}

		[Test]
		public void ShouldReturnNullIfKeyNotExist()
		{
			var cryptoKeyInfoRepository = new FakeCryptoKeyInfoRepository();
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, new Now());

			var cryptoKey = target.GetKey("bucket", "handle");

			cryptoKey.Should().Be.Null();
		}

        [Test]
	    public void ShouldReturnNoKeysForAnEmptyBucket()
	    {
            var cryptoKeyInfoRepository = new FakeCryptoKeyInfoRepository();
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, new Now());

            var cryptoKeys = target.GetKeys("bucket");

            cryptoKeys.Should().Not.Be.Null().And.Be.Empty();
	    }

        [Test]
        public void ShouldReturnKeysForBucket()
        {
			var now = new Now();
			var cryptoKeyInfo = new CryptoKeyInfo
            {
                CryptoKey = Guid.NewGuid().ToByteArray(),
				CryptoKeyExpiration = now.UtcDateTime() + TimeSpan.FromMinutes(1),
                Bucket = "bucket",
                Handle = "handle"
            };

            var cryptoKeyInfoRepository = new FakeCryptoKeyInfoRepository();
			cryptoKeyInfoRepository.Add(cryptoKeyInfo);
	        var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, now);

            var cryptoKeys = target.GetKeys(cryptoKeyInfo.Bucket)?.ToList();

            cryptoKeys.Should().Not.Be.Null().And.Not.Be.Empty();
            cryptoKeys.First().Key.Should().Be.EqualTo(cryptoKeyInfo.Handle);
            cryptoKeys.First().Value.ExpiresUtc.Should().Be.EqualTo(cryptoKeyInfo.CryptoKeyExpiration);
            cryptoKeys.First().Value.Key.Should().Have.SameSequenceAs(cryptoKeyInfo.CryptoKey);
        }

		[Test]
		public void ShouldStoreKeyAndClearExpired()
		{
			var cryptoKeyInfoRepository = new FakeCryptoKeyInfoRepository();
			
			var now = new Now();
			
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, now);

			var key = Guid.NewGuid().ToByteArray();
			var cryptoKeyExpiration = now.UtcDateTime() + TimeSpan.FromMinutes(1);
			const string bucket = "bucket";
			const string handle = "handle";
			cryptoKeyInfoRepository.Add(new CryptoKeyInfo { Bucket = bucket, Handle = "expiredhandle", CryptoKeyExpiration = now.UtcDateTime() - TimeSpan.FromHours(1) });

			target.StoreKey(bucket, handle, new CryptoKey(key, cryptoKeyExpiration));

			var keys = cryptoKeyInfoRepository.Find(bucket).ToList();
			keys.Count.Should().Be.EqualTo(1);
			var addedKey = keys.First();
			addedKey.Handle.Should().Be.EqualTo(handle);
			addedKey.Bucket.Should().Be.EqualTo(bucket);
			(addedKey.CryptoKey == key).Should().Be.True();
			addedKey.CryptoKeyExpiration.Should().Be.EqualTo(cryptoKeyExpiration);
		}

		[Test]
		public void ShouldRemoveKey()
		{
			var cryptoKeyInfoRepository = new FakeCryptoKeyInfoRepository();

			var now = new Now();
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, now);

			const string bucket = "bucket";
			const string handle = "handle";

			cryptoKeyInfoRepository.Add(new CryptoKeyInfo
			{
				CryptoKey = Guid.NewGuid().ToByteArray(),
				CryptoKeyExpiration = now.UtcDateTime() + TimeSpan.FromMinutes(1),
				Bucket = "bucket",
				Handle = "handle"

			});

			target.RemoveKey(bucket, handle);

			cryptoKeyInfoRepository.Find(bucket, handle).Should().Be.Null();
		}

		[Test]
		public void ShouldStoreNonceIfNotExists()
		{
			var nonceInfoRepository = new FakeNonceInfoRepository();
			var now = new Now();
			var testableNow = new MutableNow(DateTime.UtcNow.AddSeconds(15));
			var target = new SqlProviderApplicationStore(null, nonceInfoRepository, testableNow);

			const string context = "context";
			const string nonce = "nonce";
			var timestampUtc = now.UtcDateTime();

			var result = target.StoreNonce(context, nonce, timestampUtc);

			result.Should().Be.True();

			var addedNonce = nonceInfoRepository.Find(context, nonce, timestampUtc);
			addedNonce.Should().Not.Be.Null();
			addedNonce.Context.Should().Be.EqualTo(context);
			addedNonce.Nonce.Should().Be.EqualTo(nonce);
			addedNonce.Timestamp.Should().Be.EqualTo(timestampUtc);
		}

		[Test]
		public void ShouldNotStoreNonceIfExists()
		{
			var nonceInfoRepository = new FakeNonceInfoRepository();
			var now = new Now();
			var target = new SqlProviderApplicationStore(null, nonceInfoRepository, now);

			const string context = "context";
			const string nonce = "nonce";
			var timestampUtc = now.UtcDateTime();
			nonceInfoRepository.Add(new NonceInfo {Context = context, Nonce = nonce, Timestamp = timestampUtc});
			var result = target.StoreNonce(context, nonce, timestampUtc);

			result.Should().Be.False();

			var existingNonce = nonceInfoRepository.Find(context, nonce, timestampUtc);
			existingNonce.Should().Not.Be.Null();
			existingNonce.Context.Should().Be.EqualTo(context);
			existingNonce.Nonce.Should().Be.EqualTo(nonce);
			existingNonce.Timestamp.Should().Be.EqualTo(timestampUtc);
		}

		[Test]
		public void ShouldNotStoreNonceIfExpired()
		{
			var nonceInfoRepository = new FakeNonceInfoRepository();
			var now = new Now();
			var target = new SqlProviderApplicationStore(null, nonceInfoRepository, now);

			const string context = "context";
			const string nonce = "nonce";
			var timestampUtc = now.UtcDateTime().Subtract(TimeSpan.FromHours(1));
			var result = target.StoreNonce(context, nonce, timestampUtc);

			result.Should().Be.False();

			nonceInfoRepository.Find(context, nonce, timestampUtc).Should().Be.Null();
			
		}
	}
}