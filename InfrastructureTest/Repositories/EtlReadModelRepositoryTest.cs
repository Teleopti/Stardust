using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class EtlReadModelRepositoryTest : DatabaseTestWithoutTransaction
	{
		private EtlReadModelRepository _target;

		[Test]
		public void ShouldLoadLastAndThis()
		{
			_target = new EtlReadModelRepository(UnitOfWork);
			var bu = new BusinessUnit("name");
			bu.SetId(Guid.NewGuid());
			var model = _target.LastChangedDate(bu, "Shedules", new DateTimePeriod(2000,1,1,2001,1,1));
			Assert.That(model,Is.Not.Null);
		}

		[Test]
		public void ShouldSaveThis()
		{
			_target = new EtlReadModelRepository(UnitOfWork);
			var bu = new BusinessUnit("name");
			bu.SetId(Guid.NewGuid());
			 _target.UpdateLastChangedDate(bu,"Schedules", DateTime.Now);
		}

		[Test]
		public void ShouldGetChanged()
		{
			_target = new EtlReadModelRepository(UnitOfWork);
			var bu = new BusinessUnit("name");
			bu.SetId(Guid.NewGuid());
			Assert.That(_target.ChangedDataOnStep(DateTime.Now, bu, "Schedules"),Is.Not.Null);
		}
	}

	
}