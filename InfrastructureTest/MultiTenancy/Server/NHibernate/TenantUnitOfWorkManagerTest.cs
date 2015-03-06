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
			target.CurrentSession();
			target.HasCurrentSession().Should().Be.True();
		}

		[Test]
		public void ShouldGetSession()
		{
			target.CurrentSession()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReuseSession()
		{
			target.CurrentSession()
				.Should().Be.SameInstanceAs(target.CurrentSession());
		}

		[Test]
		public void CancelShouldRollbackTransaction()
		{
			var session = target.CurrentSession();
			var transaction = session.Transaction;
			target.CancelCurrent();
			transaction.WasRolledBack.Should().Be.True();
			session.IsConnected.Should().Be.False();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CancelOnNonExistingCurrentShouldDoNothing()
		{
			target.CancelCurrent();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CommitShouldCommitTransaction()
		{
			var session = target.CurrentSession();
			var transaction = session.Transaction;
			target.CommitCurrent();
			transaction.WasCommitted.Should().Be.True();
			session.IsConnected.Should().Be.False();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CommitOnNonExistingCurrentShouldDoNothing()
		{
			target.CommitCurrent();
			target.HasCurrentSession().Should().Be.False();
		}

		[Test]
		public void CommitOnAlreadyRolledbackShouldKeepItRolledback()
		{
			var session = target.CurrentSession();
			var transaction = session.Transaction;
			target.CancelCurrent();
			target.CommitCurrent();
			transaction.WasRolledBack.Should().Be.True();
			transaction.WasCommitted.Should().Be.False();
		}


		private TenantUnitOfWorkManager target;

		[SetUp]
		public void CreateTarget()
		{
			target = TenantUnitOfWorkManager.CreateInstanceForTest(UnitOfWorkFactory.Current.ConnectionString);
		}

		[TearDown]
		public void VerifySessionIsClosed()
		{
			if(target.HasCurrentSession())
				target.CurrentSession().Dispose();
		}
	}
}