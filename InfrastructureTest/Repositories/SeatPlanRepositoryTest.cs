using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	internal class SeatPlanRepositoryTest : RepositoryTest<ISeatPlan>
	{
		private DateOnly date = new DateOnly(2015, 10, 2);
		
		protected override ISeatPlan CreateAggregateWithCorrectBusinessUnit()
		{
			var seatPlan = new SeatPlan
			{
				Date = date,
				Status = SeatPlanStatus.InError
			};

			date = date.AddDays(1);

			return seatPlan;
		}

		protected override void VerifyAggregateGraphProperties (ISeatPlan loadedAggregateFromDatabase)
		{
			Assert.IsNotNull(loadedAggregateFromDatabase.Id);
			Assert.IsNotNull(loadedAggregateFromDatabase.Date);
			Assert.IsNotNull(loadedAggregateFromDatabase.Status);
			Assert.AreEqual(SeatPlanStatus.InError, loadedAggregateFromDatabase.Status);
		}

		protected override Repository<ISeatPlan> TestRepository (ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SeatPlanRepository(currentUnitOfWork); 
		}

		[Test]
		public void ShouldChangeSeatPlanStatus()
		{
			var seatPlan = new SeatPlan
			{
				Date = date,
				Status = SeatPlanStatus.InProgress
			};

			PersistAndRemoveFromUnitOfWork(seatPlan);

			var seatPlanRepo = new SeatPlanRepository (CurrUnitOfWork);
			var existingSeatPlan = seatPlanRepo.GetSeatPlanForDate (seatPlan.Date);
			existingSeatPlan.Status = SeatPlanStatus.Ok;

			var loaded = seatPlanRepo.Get(seatPlan.Id.GetValueOrDefault());
			Assert.AreEqual(loaded.Status, SeatPlanStatus.Ok);
			Assert.AreEqual(loaded.Id, seatPlan.Id);
		}
	}
}