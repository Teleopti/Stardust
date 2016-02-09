using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.InfrastructureTest.Authentication
{
	[DatabaseTest]
	public class NonceInfoRepositoryTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public INonceInfoRepository Target;

		[Test]
		public void ShouldFindNonce()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string context = "context";
				const string nonce = "nonce";
				var expiration = DateTime.Today;

				Target.Add(new NonceInfo
				{
					Context = context,
					Nonce = nonce,
					Timestamp = expiration
				});
				var result = Target.Find(context, nonce, expiration);
				result.Should().Not.Be.Null();
				result.Context.Should().Be.EqualTo(context);
				result.Nonce.Should().Be.EqualTo(nonce);
				result.Timestamp.Should().Be.EqualTo(expiration);
			}
		}

		[Test]
		public void ShouldReturnNullIfNonceNotExists()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string context = "context";
				const string nonce = "nonce";
				var expiration = DateTime.Today;

				var result = Target.Find(context, nonce, expiration);
				result.Should().Be.Null();
			}
		}

		[Test]
		public void ShouldClearExpiredNonces()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				const string context = "context";
				const string nonce = "nonce";
				var timestamp = DateTime.Today;

				var expired = timestamp.Subtract(TimeSpan.FromMinutes(3));
				Target.Add(new NonceInfo
				{
					Context = context,
					Nonce = nonce,
					Timestamp = expired
				});

				var nonExpired = timestamp.Add(TimeSpan.FromMinutes(3));
				Target.Add(new NonceInfo
				{
					Context = context,
					Nonce = nonce,
					Timestamp = nonExpired
				});

				Target.ClearExpired(timestamp);

				Target.Find(context, nonce, expired).Should().Be.Null();
				Target.Find(context, nonce, nonExpired).Should().Not.Be.Null();
			}
		}
	}
}