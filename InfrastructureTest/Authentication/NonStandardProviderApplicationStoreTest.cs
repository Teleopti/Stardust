using System;
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
			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository);
			var cryptoKey = target.GetKey("bucket", "handle");
			cryptoKey.Should().Not.Be.Null();
			cryptoKey.Key.Should().Have.SameSequenceAs(cryptoKeyInfo.CryptoKey);
			cryptoKey.ExpiresUtc.Should().Be.EqualTo(cryptoKeyInfo.CryptoKeyExpiration);
		}

		[Test]
		public void ShouldReturnNullIfKeyNotExist()
		{
			var cryptoKeyInfoRepository = MockRepository.GenerateMock<ICryptoKeyInfoRepository>();
			var target = new NonStandardProviderApplicationStore(cryptoKeyInfoRepository);
			var cryptoKey = target.GetKey("bucket", "handle");
			cryptoKey.Should().Be.Null();
		}
	}
}