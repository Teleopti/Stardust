using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	[TestFixture, Category("BucketB")]
	public class PurgeSettingRepositoryTest
	{
		public IPurgeSettingRepository Target;

		[Test]
		public void ShouldFindAllPrugeSettings()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = uow.FetchSession();
				session.CreateSQLQuery(
						"insert into dbo.purgeSetting ([Key], [Value]) values('test', 35)")
					.ExecuteUpdate();
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var allSettings = Target.FindAllPurgeSettings().Where(s=> s.Key == "test");
				allSettings.Count().Should().Be.EqualTo(1);
				allSettings.First().Key.Should().Be.EqualTo("test");
			}
		}
	}

	
}
