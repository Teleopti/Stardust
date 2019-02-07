using NHibernate.Envers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	public class AuditSettingRepositoryTest : AuditTest
	{
		private IAuditSettingRepository target;

		protected override void AuditSetup()
		{
			target = new AuditSettingRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		[Test]
		public void ShouldMoveScheduleFromCurrentToAuditTables()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var s = UnitOfWorkFactory.Current.CurrentUnitOfWork().FetchSession();
				s.Auditer().GetRevisions(typeof(PersonAssignment), PersonAssignment.Id.Value).Should().Not.Be.Empty();
				const string sql = "exec Auditing.InitAuditTables";
				uow.FetchSession().CreateSQLQuery(sql).ExecuteUpdate();
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var s = UnitOfWorkFactory.Current.CurrentUnitOfWork().FetchSession();
				s.Auditer().GetRevisions(typeof(PersonAssignment), PersonAssignment.Id.Value).Should().Not.Be.Empty();
			}
		}

		[Test]
		public void ShouldThrowIfNotExists()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				UnitOfWorkFactory.Current.CurrentUnitOfWork().FetchSession().CreateQuery("delete from AuditSetting").ExecuteUpdate();
				Assert.Throws<DataSourceException>(() => target.Read());				
			}
		}

		[Test]
		public void ShouldReturnTheOneIfExists()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repWithExplicitUow = new AuditSettingRepository(new ThisUnitOfWork(uow));
				repWithExplicitUow.Read().Should().Not.Be.Null();
			}
		}
	}
}