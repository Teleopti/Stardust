using System;
using NUnit.Framework;
using SharpTestsEx;
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
	}
}