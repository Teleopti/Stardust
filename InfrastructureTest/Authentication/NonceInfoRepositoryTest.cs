using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Authentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Authentication
{
	[TestFixture]
	public class NonceInfoRepositoryTest : DatabaseTest
	{
		[Test]
		public void ShouldFindNonce()
		{
			var target = new NonceInfoRepository(CurrUnitOfWork);
			const string context = "context";
			const string nonce = "nonce";
			var expiration = DateTime.Today;
			PersistAndRemoveFromUnitOfWork(new NonceInfo
			{
				Context = context,
				Nonce = nonce,
				Timestamp = expiration
			});
			var result = target.Find(context, nonce, expiration);
			result.Should().Not.Be.Null();
			result.Context.Should().Be.EqualTo(context);
			result.Nonce.Should().Be.EqualTo(nonce);
			result.Timestamp.Should().Be.EqualTo(expiration);
		}

		[Test]
		public void ShouldReturnNullIfNonceNotExists()
		{
			var target = new NonceInfoRepository(CurrUnitOfWork);
			const string context = "context";
			const string nonce = "nonce";
			var expiration = DateTime.Today;
			
			var result = target.Find(context, nonce, expiration);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldStoreNonce()
		{
			var target = new NonceInfoRepository(CurrUnitOfWork);
			const string context = "context";
			const string nonce = "nonce";
			var expiration = DateTime.Today;

			var nonceInfo = new NonceInfo
			{
				Context = context,
				Nonce = nonce,
				Timestamp = expiration
			};
			target.Add(nonceInfo);

			UnitOfWork.Contains(nonceInfo);
		}
	}
}