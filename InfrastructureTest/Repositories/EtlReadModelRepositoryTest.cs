using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class EtlReadModelRepositoryTest : DatabaseTest
	{
		private MockRepository _mocks;
		private EtlReadModelRepository _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new EtlReadModelRepository(UnitOfWork);
		}

		[Test]
		public void ShouldLoadLastAndThis()
		{
			var bu = new BusinessUnit("name");
			bu.SetId(Guid.NewGuid());
			var model = _target.LastChangedDate(bu, "Shedules");
			Assert.That(model,Is.Not.Null);
		}

		[Test]
		public void ShouldSaveThis()
		{
			var bu = new BusinessUnit("name");
			bu.SetId(Guid.NewGuid());
			 _target.UpdateLastChangedDate(bu,"Schedules", DateTime.Now);
		}

		[Test]
		public void ShouldGetChanged()
		{
			var bu = new BusinessUnit("name");
			bu.SetId(Guid.NewGuid());
			Assert.That(_target.ChangedDataOnStep(DateTime.Now, bu, "Schedules"),Is.Not.Null);
		}
	}

	
}