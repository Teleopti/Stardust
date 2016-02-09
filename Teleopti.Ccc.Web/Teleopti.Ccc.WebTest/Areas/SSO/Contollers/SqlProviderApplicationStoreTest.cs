using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.SSO.Contollers
{
	[TestFixture]
	public class SqlProviderApplicationStoreTest
	{
		[Test]
		public void ShouldGetKey()
		{
			var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();
			var now = new Now();
			var cryptoKeyInfo = new CryptoKeyInfo
			{
				CryptoKey = Guid.NewGuid().ToByteArray(),
				CryptoKeyExpiration = now.UtcDateTime()
			};
			cryptoKeyInfoRepository.Stub(x => x.Find("bucket", "handle")).Return(cryptoKeyInfo);
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, now);
			var cryptoKey = target.GetKey("bucket", "handle");
			cryptoKey.Should().Not.Be.Null();
			cryptoKey.Key.Should().Have.SameSequenceAs(cryptoKeyInfo.CryptoKey);
			cryptoKey.ExpiresUtc.Should().Be.EqualTo(cryptoKeyInfo.CryptoKeyExpiration);
		}

		[Test]
		public void ShouldReturnNullIfKeyNotExist()
		{
			var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, new Now());
			var cryptoKey = target.GetKey("bucket", "handle");
			cryptoKey.Should().Be.Null();
		}

        [Test]
	    public void ShouldReturnNoKeysForAnEmptyBucket()
	    {
            var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();

            cryptoKeyInfoRepository.Stub(x => x.Find("bucket")).Return(new List<CryptoKeyInfo>());
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
				CryptoKeyExpiration = now.UtcDateTime(),
                Bucket = "bucket",
                Handle = "handle"
            };

            var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();

            cryptoKeyInfoRepository.Stub(x => x.Find(cryptoKeyInfo.Bucket)).Return(new List<CryptoKeyInfo> {cryptoKeyInfo});
	        var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, now);
            var cryptoKeys = target.GetKeys(cryptoKeyInfo.Bucket);
            cryptoKeys.Should().Not.Be.Null().And.Not.Be.Empty();
            cryptoKeys.First().Key.Should().Be.EqualTo(cryptoKeyInfo.Handle);
            cryptoKeys.First().Value.ExpiresUtc.Should().Be.EqualTo(cryptoKeyInfo.CryptoKeyExpiration);
            cryptoKeys.First().Value.Key.Should().Have.SameSequenceAs(cryptoKeyInfo.CryptoKey);
        }

		[Test]
		public void ShouldStoreKey()
		{
			var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();

			var now = new Now();
			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, now);

			var key = Guid.NewGuid().ToByteArray();
			var cryptoKeyExpiration = now.UtcDateTime();
			const string bucket = "bucket";
			const string handle = "handle";
			target.StoreKey(bucket, handle, new CryptoKey(key, cryptoKeyExpiration));

			cryptoKeyInfoRepository.AssertWasCalled(x => x.Add(Arg<CryptoKeyInfo>.Matches(
				c => c.Bucket == bucket && c.Handle == handle && c.CryptoKey == key && c.CryptoKeyExpiration == cryptoKeyExpiration
				)));
		}

		[Test]
		public void ShouldRemoveKey()
		{
			var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();

			var target = new SqlProviderApplicationStore(cryptoKeyInfoRepository, null, new Now());

			const string bucket = "bucket";
			const string handle = "handle";
			target.RemoveKey(bucket, handle);

			cryptoKeyInfoRepository.AssertWasCalled(x => x.Remove(bucket, handle));
		}

		[Test]
		public void ShouldStoreNonceIfNotExists()
		{
			var nonceInfoRepository = MockRepository.GenerateMock<INonceInfoRepository>();
			var now = new Now();
			var testableNow = new TestableNow(DateTime.UtcNow.AddSeconds(15));
			var target = new SqlProviderApplicationStore(null, nonceInfoRepository, testableNow);

			const string context = "context";
			const string nonce = "nonce";
			var timestampUtc = now.UtcDateTime();
			var result = target.StoreNonce(context, nonce, timestampUtc);

			nonceInfoRepository.AssertWasCalled(x => x.Find(context, nonce, timestampUtc));
			nonceInfoRepository.AssertWasCalled(x => x.ClearExpired(testableNow.UtcDateTime().Subtract(TimeSpan.FromMinutes(5))));

			nonceInfoRepository.AssertWasCalled(x => x.Add(Arg<NonceInfo>.Matches(
				c => c.Context == context && c.Nonce == nonce && c.Timestamp == timestampUtc)));

			result.Should().Be.True();
		}

		[Test]
		public void ShouldNotStoreNonceIfExists()
		{
			var nonceInfoRepository = MockRepository.GenerateMock<INonceInfoRepository>();
			var now = new Now();
			var target = new SqlProviderApplicationStore(null, nonceInfoRepository, now);

			const string context = "context";
			const string nonce = "nonce";
			var timestampUtc = now.UtcDateTime();
			nonceInfoRepository.Stub(x => x.Find(context, nonce, timestampUtc)).Return(new NonceInfo());
			var result = target.StoreNonce(context, nonce, timestampUtc);

			nonceInfoRepository.AssertWasCalled(x => x.Find(context, nonce, timestampUtc));

			nonceInfoRepository.AssertWasNotCalled(x => x.Add(Arg<NonceInfo>.Matches(
				c => c.Context == context && c.Nonce == nonce && c.Timestamp == timestampUtc)));

			result.Should().Be.False();
		}

		[Test]
		public void ShouldNotStoreNonceIfExpired()
		{
			var nonceInfoRepository = MockRepository.GenerateMock<INonceInfoRepository>();
			var now = new Now();
			var target = new SqlProviderApplicationStore(null, nonceInfoRepository, now);

			const string context = "context";
			const string nonce = "nonce";
			var timestampUtc = now.UtcDateTime().Subtract(TimeSpan.FromHours(1));
			var result = target.StoreNonce(context, nonce, timestampUtc);

			nonceInfoRepository.AssertWasNotCalled(x => x.Find(context, nonce, timestampUtc));
			nonceInfoRepository.AssertWasNotCalled(x => x.Add(Arg<NonceInfo>.Matches(
				c => c.Context == context && c.Nonce == nonce && c.Timestamp == timestampUtc)));
			result.Should().Be.False();
		}
	}
}