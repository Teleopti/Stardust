using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Authentication;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Authentication
{
	[TestFixture]
	public class CryptoKeyInfoRepositoryTest : DatabaseTest
	{
		[Test]
		public void ShouldFindKeyByBucketAndHandle()
		{
			const string bucket = "bucket";
			const string handle = "handle";
			var cryptoKey = Guid.NewGuid().ToByteArray();
			var cryptoKeyExpiration = DateTime.Today;
			PersistAndRemoveFromUnitOfWork(new CryptoKeyInfo
			{
				Bucket = bucket,
				Handle = handle,
				CryptoKey = cryptoKey,
				CryptoKeyExpiration = cryptoKeyExpiration
			});
			var target = new CryptoKeyInfoRepository(CurrUnitOfWork);
			var result = target.Find(bucket, handle);

			result.Bucket.Should().Be.EqualTo(bucket);
			result.Handle.Should().Be.EqualTo(handle);
			result.CryptoKey.Should().Have.SameSequenceAs(cryptoKey);
			result.CryptoKeyExpiration.Should().Be.EqualTo(cryptoKeyExpiration);
		}


		[Test]
		public void ShouldFindKeysByBucket()
		{
			const string bucket = "bucket";
			const string handle = "handle";
			const string handle2 = "handle2";
			var cryptoKey = Guid.NewGuid().ToByteArray();
			var cryptoKeyExpiration = DateTime.Today;
			PersistAndRemoveFromUnitOfWork(new CryptoKeyInfo
			{
				Bucket = bucket,
				Handle = handle,
				CryptoKey = cryptoKey,
				CryptoKeyExpiration = cryptoKeyExpiration
			});

			PersistAndRemoveFromUnitOfWork(new CryptoKeyInfo
			{
				Bucket = bucket,
				Handle = handle2,
				CryptoKey = cryptoKey,
				CryptoKeyExpiration = cryptoKeyExpiration
			});
			var target = new CryptoKeyInfoRepository(CurrUnitOfWork);
			var result = target.Find(bucket);

			result.Count().Should().Be.EqualTo(2);
			result.ForEach(x => x.Bucket.Should().Be.EqualTo(bucket));
			result.ForEach(x => x.CryptoKey.Should().Have.SameSequenceAs(cryptoKey));
			result.ForEach(x => x.CryptoKeyExpiration.Should().Be.EqualTo(cryptoKeyExpiration));
			result.Any(x=>x.Handle==handle).Should().Be.True();
			result.Any(x=>x.Handle==handle2).Should().Be.True();
		}


		[Test]
		public void ShouldAdd()
		{
			const string bucket = "bucket";
			const string handle = "handle";
			var cryptoKey = Guid.NewGuid().ToByteArray();
			var cryptoKeyExpiration = DateTime.Today;
			var target = new CryptoKeyInfoRepository(CurrUnitOfWork);
			var cryptoKeyInfo = new CryptoKeyInfo
			{
				Bucket = bucket,
				Handle = handle,
				CryptoKey = cryptoKey,
				CryptoKeyExpiration = cryptoKeyExpiration
			};
			target.Add(cryptoKeyInfo);

			Assert.IsTrue(UnitOfWork.Contains(cryptoKeyInfo));
		}

		[Test]
		public void ShouldRemove()
		{
			const string bucket = "bucket";
			const string handle = "handle";
			var cryptoKey = Guid.NewGuid().ToByteArray();
			var cryptoKeyExpiration = DateTime.Today;
			PersistAndRemoveFromUnitOfWork(new CryptoKeyInfo
			{
				Bucket = bucket,
				Handle = handle,
				CryptoKey = cryptoKey,
				CryptoKeyExpiration = cryptoKeyExpiration
			});
			var target = new CryptoKeyInfoRepository(CurrUnitOfWork);
			target.Find(bucket, handle).Should().Not.Be.Null();
			target.Remove(bucket, handle);

			Session.Flush();

			target.Find(bucket, handle).Should().Be.Null();
		}
	}
}