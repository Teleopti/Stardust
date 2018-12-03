using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class EtlJobStatusRepositoryTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldLoadJobStatusModel()
		{
			var target = new EtlJobStatusRepository(CurrentDataSource.Make());
			var model = target.Load(new DateOnly(2010,1,1), true);
			Assert.That(model,Is.Not.Null);
		}
	}
}