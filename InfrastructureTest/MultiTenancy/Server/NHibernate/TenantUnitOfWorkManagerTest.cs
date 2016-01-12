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
			target.EnsureUnitOfWorkIsStarted();
			target.HasCurrentSession().Should().Be.True();
		}

		[Test]
		public void ShouldGetSession()
		{
			target.EnsureUnitOfWorkIsStarted();
			target.CurrentSession()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReuseSession()
		{
			target.EnsureUnitOfWorkIsStarted();
			target.CurrentSession()
				.Should().Be.SameInstanceAs(target.CurrentSession());
		}

		[Test]
		public void CancelShouldRollbackTransaction()
		{
			target.EnsureUnitOfWorkIsStarted();
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
			target.EnsureUnitOfWorkIsStarted();
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
			target.EnsureUnitOfWorkIsStarted();
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
			using (target.EnsureUnitOfWorkIsStarted())
			{
				target.HasCurrentSession().Should().Be.True();
				using (target.EnsureUnitOfWorkIsStarted())
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
			using (target.EnsureUnitOfWorkIsStarted())
			{
				var session = target.CurrentSession();
				using (target.EnsureUnitOfWorkIsStarted())
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
			target = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString);
		}

		[TearDown]
		public void VerifySessionIsClosed()
		{
			target.CancelAndDisposeCurrent();
		}
	}
}