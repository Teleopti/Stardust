using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class RequeueHangfireRepositoryTest
	{
		public IRequeueHangfireRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		[Test]
		public void ShouldGetRequeueCommands()
		{
			createEntry();

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetUnhandledRequeueCommands());
			result.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldMarkCommandDone()
		{
			createEntry();

			var requeueCommand = WithAnalyticsUnitOfWork.Get(() => Target.GetUnhandledRequeueCommands()).First();

			WithAnalyticsUnitOfWork.Do(() => Target.MarkAsCompleted(requeueCommand));

			WithAnalyticsUnitOfWork.Get(() => Target.GetUnhandledRequeueCommands()).Should().Be.Empty();
		}

		private void createEntry()
		{
			WithAnalyticsUnitOfWork.Do(uow =>
			{
				uow.Current().FetchSession().CreateSQLQuery(@"
					DELETE FROM [dbo].[hangfire_requeue]
				").ExecuteUpdate();
			});
			WithAnalyticsUnitOfWork.Do(uow =>
			{
				uow.Current().FetchSession().CreateSQLQuery(@"
					INSERT INTO [dbo].[hangfire_requeue]
					VALUES (NEWID(), 'Teleopti.Ccc.Domain.ApplicationLayer.Events.TenantDayTickEvent', null, 0, null)
				").ExecuteUpdate();
			});
		}
	}
}