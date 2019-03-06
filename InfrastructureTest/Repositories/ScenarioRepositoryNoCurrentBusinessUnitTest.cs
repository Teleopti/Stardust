using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class ScenarioRepositoryNoCurrentBusinessUnitTest
	{
		public IScenarioRepository repository;
		public WithUnitOfWork UnitOfWork;
		public ILogOnOffContext Context;
		public IDataSourceScope DataSourceScope;
		public ICurrentDataSource DataSource;

		[Test]
		public void ShouldThrowIfDefaultScenarioDoesNotExistAndNoCurrentBusinessUnit()
		{
			var dataSource = DataSource.Current();
			UnitOfWork.Do(() =>
			{
				repository.LoadAll().ForEach(s =>
				{
					((IDeleteTag) s).SetDeleted();
				});
			});
			Context.Logout();

			using (DataSourceScope.OnThisThreadUse(dataSource))
			{
				UnitOfWork.Do(() =>
				{
					Assert.Throws<NoDefaultScenarioException>(() =>
					{
						repository.LoadDefaultScenario();
					});
				});
			}

		}
	}
}