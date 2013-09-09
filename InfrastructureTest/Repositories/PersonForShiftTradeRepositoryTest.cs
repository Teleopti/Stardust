using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class PersonForShiftTradeRepositoryTest : DatabaseTest
	{
		private PersonForShiftTradeRepository _target;

		[Test]
		public void ShouldLoadPersonInMyTeamForShiftTrade()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonForShiftTradeRepository(uow);
				_target.GetPersonForShiftTrade(new DateOnly(2012, 12, 27), Guid.NewGuid());
			}
		}

		[Test]
		public void ShouldLoadPersonInAnyTeamForShiftTrade()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonForShiftTradeRepository(uow);
				_target.GetPersonForShiftTrade(new DateOnly(2012, 12, 27), null);
			}
		}
	}
}
