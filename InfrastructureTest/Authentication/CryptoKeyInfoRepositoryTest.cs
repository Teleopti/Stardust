using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.InfrastructureTest.Authentication
{
	[DatabaseTest]
	[Category("LongRunning")]
	public class CryptoKeyInfoRepositoryTest 
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICryptoKeyInfoRepository Target;

		[Test]
		public void ShouldFindKeyByBucketAndHandle()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string bucket = "bucket";
				const string handle = "handle";
				var cryptoKey = Guid.NewGuid().ToByteArray();
				var cryptoKeyExpiration = DateTime.Today+TimeSpan.FromDays(1);
				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = cryptoKeyExpiration
				});

				var result = Target.Find(bucket, handle);

				result.Bucket.Should().Be.EqualTo(bucket);
				result.Handle.Should().Be.EqualTo(handle);
				result.CryptoKey.Should().Have.SameSequenceAs(cryptoKey);
				result.CryptoKeyExpiration.Should().Be.EqualTo(cryptoKeyExpiration);
			}
		}

		[Test]
		public void ShouldNotFindExpiredKeyByBucketAndHandle()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string bucket = "bucket";
				const string handle = "handle";
				var cryptoKey = Guid.NewGuid().ToByteArray();
				var cryptoKeyExpiration = DateTime.Today-TimeSpan.FromDays(1);
				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = cryptoKeyExpiration
				});

				var result = Target.Find(bucket, handle);

				result.Should().Be.Null();
			}
		}


		[Test]
		public void ShouldFindKeysByBucket()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string bucket = "bucket";
				const string handle = "handle";
				const string handle2 = "handle2";
				var cryptoKey = Guid.NewGuid().ToByteArray();
				var cryptoKeyExpiration = DateTime.Today + TimeSpan.FromDays(1);
				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = cryptoKeyExpiration
				});

				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle2,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = cryptoKeyExpiration
				});

				var result = Target.Find(bucket).ToList();

				result.Count.Should().Be.EqualTo(2);
				result.ForEach(x => x.Bucket.Should().Be.EqualTo(bucket));
				result.ForEach(x => x.CryptoKey.Should().Have.SameSequenceAs(cryptoKey));
				result.ForEach(x => x.CryptoKeyExpiration.Should().Be.EqualTo(cryptoKeyExpiration));
				result.Any(x => x.Handle == handle).Should().Be.True();
				result.Any(x => x.Handle == handle2).Should().Be.True();
			}
			
		}

		[Test]
		public void ShouldNotFindExpiredKeysByBucket()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string bucket = "bucket";
				const string handle = "handle";
				const string handle2 = "handle2";
				var cryptoKey = Guid.NewGuid().ToByteArray();
				var notExpired = DateTime.Today + TimeSpan.FromDays(1);
				var expired = DateTime.Today - TimeSpan.FromDays(1);

				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = notExpired
				});
				
				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle2,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = expired
				});

				var result = Target.Find(bucket).ToList();

				result.Count.Should().Be.EqualTo(1);
				result.First().Bucket.Should().Be.EqualTo(bucket);
				result.First().Handle.Should().Be.EqualTo(handle);
				result.First().CryptoKeyExpiration.Should().Be.EqualTo(notExpired);
				result.First().CryptoKey.Should().Have.SameSequenceAs(cryptoKey);
			}

		}

		[Test]
		public void ShouldRemove()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string bucket = "bucket";
				const string handle = "handle";
				var cryptoKey = Guid.NewGuid().ToByteArray();
				var cryptoKeyExpiration = DateTime.Today + TimeSpan.FromDays(1);
				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = cryptoKeyExpiration
				});
				Target.Find(bucket, handle).Should().Not.Be.Null();
				Target.Remove(bucket, handle);

				Target.Find(bucket, handle).Should().Be.Null();
			}
		}

		[Test]
		public void ShouldClearExpired()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string bucket = "bucket";
				const string handle = "handle";
				var cryptoKey = Guid.NewGuid().ToByteArray();
				var cryptoKeyExpiration = DateTime.Today+TimeSpan.FromDays(1);
				Target.Add(new CryptoKeyInfo
				{
					Bucket = bucket,
					Handle = handle,
					CryptoKey = cryptoKey,
					CryptoKeyExpiration = cryptoKeyExpiration
				});

				Target.ClearExpired(DateTime.Today + TimeSpan.FromDays(2));

				Target.Find(bucket, handle).Should().Be.Null();
			}
		}
	}
}