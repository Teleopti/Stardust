using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkManagerTest
	{
		[Test]
		public void InstansationOfSessionShouldBeLazy()
		{
			target.HasCurrentSession().Should().Be.False();
			target.Start();
			target.HasCurrentSession().Should().Be.True();
		}

		[Test]
		public void ShouldGetSession()
		{
			target.Start();
			target.CurrentSession()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReuseSession()
		{
			target.Start();
			target.CurrentSession()
				.Should().Be.SameInstanceAs(target.CurrentSession());
		}

		[Test]
		public void CancelShouldRollbackTransaction()
		{
			target.Start();
			var session = target.CurrentSession();
			var transaction = session.Transaction;
			target.CancelAndDisposeCurrent();
			transaction.WasRolledBack.Should().Be.True();
			session.IsConnected.Should().Be.False();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CancelOnNonExistingCurrentShouldDoNothing()
		{
			target.CancelAndDisposeCurrent();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CommitShouldCommitTransaction()
		{
			target.Start();
			var session = target.CurrentSession();
			var transaction = session.Transaction;
			target.CommitAndDisposeCurrent();
			transaction.WasCommitted.Should().Be.True();
			session.IsConnected.Should().Be.False();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CommitOnNonExistingCurrentShouldDoNothing()
		{
			target.CommitAndDisposeCurrent();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CommitOnAlreadyRolledbackShouldKeepItRolledback()
		{
			target.Start();
			var session = target.CurrentSession();
			var transaction = session.Transaction;
			target.CancelAndDisposeCurrent();
			target.CommitAndDisposeCurrent();
			transaction.WasRolledBack.Should().Be.True();
			transaction.WasCommitted.Should().Be.False();
		}

		[Test]
		public void NonStartedShouldThrow()
		{
			Assert.Throws<HibernateException>(() =>
				target.CurrentSession());
		}

		[Test]
		public void ShouldWorkWithNestedUnitOfWorks()
		{
			target.HasCurrentSession().Should().Be.False();
			using (target.Start())
			{
				target.HasCurrentSession().Should().Be.True();
				using (target.Start())
				{
					target.HasCurrentSession().Should().Be.True();
				}
				target.HasCurrentSession().Should().Be.True();
			}
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void NestedUnitOfWorksShouldShareSession()
		{
			using (target.Start())
			{
				var session = target.CurrentSession();
				using (target.Start())
				{
					target.CurrentSession()
						.Should().Be.SameInstanceAs(session);
				}
			}
		}

		private TenantUnitOfWorkManager target;

		[SetUp]
		public void CreateTarget()
		{
			target = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(UnitOfWorkFactory.Current.ConnectionString);
		}

		[TearDown]
		public void VerifySessionIsClosed()
		{
			if(target.HasCurrentSession())
				target.CurrentSession().Dispose();
		}
	}
}