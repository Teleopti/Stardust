using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Authentication;
using Teleopti.Ccc.Infrastructure.Authentication;

namespace Teleopti.Ccc.InfrastructureTest.Authentication
{
	[TestFixture]
	public class NonStandardProviderApplicationStoreTest
	{
		[Test]
		public void ShouldGetKey()
		{
			var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();
			var cryptoKeyInfo = new CryptoKeyInfo
			{
				CryptoKey = Guid.NewGuid().ToByteArray(),
				CryptoKeyExpiration = DateTime.UtcNow
			};
			cryptoKeyInfoRepository.Stub(x => x.Find("bucket", "handle")).Return(cryptoKeyInfo);
			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository, null);
			var cryptoKey = target.GetKey("bucket", "handle");
			cryptoKey.Should().Not.Be.Null();
			cryptoKey.Key.Should().Have.SameSequenceAs(cryptoKeyInfo.CryptoKey);
			cryptoKey.ExpiresUtc.Should().Be.EqualTo(cryptoKeyInfo.CryptoKeyExpiration);
		}

		[Test]
		public void ShouldReturnNullIfKeyNotExist()
		{
			var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();
			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository, null);
			var cryptoKey = target.GetKey("bucket", "handle");
			cryptoKey.Should().Be.Null();
		}

        [Test]
	    public void ShouldReturnNoKeysForAnEmptyBucket()
	    {
            var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();

            cryptoKeyInfoRepository.Stub(x => x.Find("bucket")).Return(new List<CryptoKeyInfo>());
			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository, null);
            var cryptoKeys = target.GetKeys("bucket");
            cryptoKeys.Should().Not.Be.Null().And.Be.Empty();
	    }

        [Test]
        public void ShouldReturnKeysForBucket()
        {
            var cryptoKeyInfo = new CryptoKeyInfo
            {
                CryptoKey = Guid.NewGuid().ToByteArray(),
                CryptoKeyExpiration = DateTime.UtcNow,
                Bucket = "bucket",
                Handle = "handle"
            };

            var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();

            cryptoKeyInfoRepository.Stub(x => x.Find(cryptoKeyInfo.Bucket)).Return(new List<CryptoKeyInfo> {cryptoKeyInfo});
			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository, null);
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

			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository, null);

			var key = Guid.NewGuid().ToByteArray();
			var cryptoKeyExpiration = DateTime.UtcNow;
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

			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository, null);

			const string bucket = "bucket";
			const string handle = "handle";
			target.RemoveKey(bucket, handle);

			cryptoKeyInfoRepository.AssertWasCalled(x => x.Remove(bucket, handle));
		}

		[Test]
		public void ShouldStoreNonce()
		{
			var nonceInfoRepository = MockRepository.GenerateMock<INonceInfoRepository>();
			var target = new NonStandardProviderApplicationStore(null, nonceInfoRepository);

			var result = target.StoreNonce("context", "nonce", DateTime.UtcNow);



		}
	}

	
}