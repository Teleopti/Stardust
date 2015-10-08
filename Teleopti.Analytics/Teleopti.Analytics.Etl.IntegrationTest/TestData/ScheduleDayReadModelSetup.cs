using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public class ScheduleDayReadModelSetup: IDataSetup
	{
		public ScheduleDayReadModel Model;

		public void Apply(IUnitOfWork uow)
		{
			var period = new DateOnlyPeriod(Model.BelongsToDate, Model.BelongsToDate);
			var rep = new ScheduleDayReadModelRepository(new ThisUnitOfWork(uow));
			rep.ClearPeriodForPerson(period,Model.PersonId);
			rep.SaveReadModel(Model);
		}
	}
}